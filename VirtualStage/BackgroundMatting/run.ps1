conda activate bgmatting
$Env:CUDA_VISIBLE_DEVICES=0
python .\bg_matting.py `
  --name "Mitra" `
  --input D:\mitra\ `
  --output_dir D:\mitra\output\ `
  --fixed_threshold "640," `
  --start 34 `
  --duration 234
conda deactivate