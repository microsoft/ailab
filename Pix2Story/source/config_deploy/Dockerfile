RUN ln -s /usr/local/cuda-9.1/lib64/libcublas.so.9.1 /usr/lib/x86_64-linux-gnu/libcublas.so
RUN ln -s /usr/local/cuda-9.1/lib64/libnvrtc-builtins.so.9.1 /usr/lib/x86_64-linux-gnu/libnvrtc-builtins.so
RUN ln -s /usr/local/cuda-9.1/lib64/libnvrtc.so.9.1 /usr/lib/x86_64-linux-gnu/libnvrtc.so
RUN apt-get update && apt-get install -y gfortran git cmake wget liblapack-dev libopenblas-dev libglib2.0-0 libxrender1 libxtst6 libxi6 python-pip python-six
RUN apt-get install -y --no-install-recommends libcudnn7=7.3.1.20-1+cuda9.2 libcudnn7-dev=7.3.1.20-1+cuda9.2 && apt-mark hold libcudnn7 && rm -rf /var/lib/apt/lists/*
RUN cd /root && git clone https://github.com/Theano/libgpuarray.git && cd libgpuarray && mkdir Build && cd Build && cmake .. -DCMAKE_BUILD_TYPE=Release && make -j"$(nproc)" && make install
RUN conda install mkl-service
RUN pip install theano 
RUN conda install pygpu
RUN pip install --upgrade https://github.com/Lasagne/Lasagne/archive/master.zip
ENV MKL_THREADING_LAYER=GNU
RUN wget -qO- "<include files cudnn nvidia>" | tar xvz
RUN mkdir /usr/local/cuda-9.1/targets/x86_64-linux/include/ && cp -r include/* /usr/local/cuda-9.1/targets/x86_64-linux/include
RUN ln -s /usr/local/cuda-9.1/targets/x86_64-linux/include/ /usr/local/cuda-9.1/include
RUN printf "[global]\ndevice=cuda\nfloatX=float32\noptimizer_including=cudnn\n[lib]\ncnmem=0.1\n[nvcc]\nfastmath=True\n[cuda]\nroot=/usr/local/cuda\ninclude_path=/usr/local/cuda/include" > ~/.theanorc
ENV THEANO_FLAGS="contexts=dev0->cuda0;dev1->cuda1"
ENV WORKER_TIMEOUT="500"