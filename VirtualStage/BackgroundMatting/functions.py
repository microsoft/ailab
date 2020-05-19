import numpy as np
import torch
import torchvision
import cv2, pdb


def composite4(fg, bg, a):
	fg = np.array(fg, np.float32)
	alpha= np.expand_dims(a / 255,axis=2)
	im = alpha * fg + (1 - alpha) * bg
	im = im.astype(np.uint8)
	return im

def compose_image_withshift(alpha_pred,fg_pred,bg,seg):

    image_sh=torch.zeros(fg_pred.shape).cuda()

    for t in range(0,fg_pred.shape[0]):
        al_tmp=to_image(seg[t,...]).squeeze(2)
        where = np.array(np.where((al_tmp>0.1).astype(np.float32)))
        x1, y1 = np.amin(where, axis=1)
        x2, y2 = np.amax(where, axis=1)

        #select shift
        n=np.random.randint(-(y1-10),al_tmp.shape[1]-y2-10)
        #n positive indicates shift to right
        alpha_pred_sh=torch.cat((alpha_pred[t,:,:,-n:],alpha_pred[t,:,:,:-n]),dim=2)
        fg_pred_sh=torch.cat((fg_pred[t,:,:,-n:],fg_pred[t,:,:,:-n]),dim=2)

        alpha_pred_sh=(alpha_pred_sh+1)/2

        image_sh[t,...]=fg_pred_sh*alpha_pred_sh + (1-alpha_pred_sh)*bg[t,...]

    return torch.autograd.Variable(image_sh.cuda())

def get_bbox(mask,R,C):
    where = np.array(np.where(mask))
    x1, y1 = np.amin(where, axis=1)
    x2, y2 = np.amax(where, axis=1)

    bbox_init=[x1,y1,np.maximum(x2-x1,y2-y1),np.maximum(x2-x1,y2-y1)]


    bbox=create_bbox(bbox_init,(R,C))

    return bbox

def crop_images(crop_list,reso,bbox):

    for i in range(0,len(crop_list)):
        img=crop_list[i]
        if img.ndim>=3:
            img_crop=img[bbox[0]:bbox[0]+bbox[2],bbox[1]:bbox[1]+bbox[3],...]; img_crop=cv2.resize(img_crop,reso)
        else:
            img_crop=img[bbox[0]:bbox[0]+bbox[2],bbox[1]:bbox[1]+bbox[3]]; img_crop=cv2.resize(img_crop,reso)
        crop_list[i]=img_crop

    return crop_list

def create_bbox(bbox_init,sh):

    w=np.maximum(bbox_init[2],bbox_init[3])

    x1=bbox_init[0]-0.1*w
    y1=bbox_init[1]-0.1*w

    x2=bbox_init[0]+1.1*w
    y2=bbox_init[1]+1.1*w

    if x1<0: x1=0
    if y1<0: y1=0
    if x2>=sh[0]: x2=sh[0]-1
    if y2>=sh[1]: y2=sh[1]-1

    bbox=np.around([x1,y1,x2-x1,y2-y1]).astype('int')

    return bbox

def uncrop(alpha,bbox,R=720,C=1280):


    alpha=cv2.resize(alpha,(bbox[3],bbox[2]))

    if alpha.ndim==2:
        alpha_uncrop=np.zeros((R,C))
        alpha_uncrop[bbox[0]:bbox[0]+bbox[2],bbox[1]:bbox[1]+bbox[3]]=alpha
    else:
        alpha_uncrop=np.zeros((R,C,3))
        alpha_uncrop[bbox[0]:bbox[0]+bbox[2],bbox[1]:bbox[1]+bbox[3],:]=alpha


    return alpha_uncrop.astype(np.uint8)


def to_image(rec0):
    rec0=((rec0.data).cpu()).numpy()
    rec0=(rec0+1)/2
    rec0=rec0.transpose((1,2,0))
    rec0[rec0>1]=1
    rec0[rec0<0]=0
    return rec0

def write_tb_log(image,tag,log_writer,i):
    # image1
    output_to_show = image.cpu().data[0:4,...]
    output_to_show = (output_to_show + 1)/2.0
    grid = torchvision.utils.make_grid(output_to_show,nrow=4)

    log_writer.add_image(tag, grid, i + 1)

