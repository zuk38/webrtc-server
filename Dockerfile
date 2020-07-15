FROM node:10-alpine3.10

RUN apk update && \
    apk add graphicsmagick ffmpeg ffmpeg-dev ghostscript ghostscript-dev make cmake g++ gcc boost boost-dev libmad libmad-dev gd gd-dev libgd libid3tag libid3tag-dev libsndfile libsndfile-dev git && \
    rm -Rf /var/cache/apk/* /var/lib/apk/* /etc/apk/cache/*

# build audiowaveform from source

RUN apk add git make cmake gcc g++ libmad-dev libid3tag-dev libsndfile-dev gd-dev boost-dev libgd libpng-dev zlib-dev
RUN apk add libpng-static boost-static

RUN apk add autoconf automake libtool gettext
RUN wget https://github.com/xiph/flac/archive/1.3.3.tar.gz
RUN tar xzf 1.3.3.tar.gz
RUN cd flac-1.3.3/ && ./autogen.sh
RUN cd flac-1.3.3/ && ./configure --enable-shared=no
RUN cd flac-1.3.3/ && make
RUN cd flac-1.3.3/ && make install

RUN git clone https://github.com/bbc/audiowaveform.git
RUN mkdir audiowaveform/build/
RUN cd audiowaveform/build/ && cmake -D ENABLE_TESTS=0 -D BUILD_STATIC=1 ..
RUN cd audiowaveform/build/ && make
RUN cd audiowaveform/build/ && make install

#install phantomJS

WORKDIR /tmp

RUN apk add --update --no-cache curl &&\
  cd /tmp && curl -Ls https://github.com/dustinblackman/phantomized/releases/download/2.1.1/dockerized-phantomjs.tar.gz | tar xz &&\
  cp -R lib lib64 / &&\
  cp -R usr/lib/x86_64-linux-gnu /usr/lib &&\
  cp -R usr/share/fonts /usr/share &&\
  cp -R etc/fonts /etc &&\
  curl -k -Ls https://bitbucket.org/ariya/phantomjs/downloads/phantomjs-2.1.1-linux-x86_64.tar.bz2 | tar -jxf - &&\
  cp phantomjs-2.1.1-linux-x86_64/bin/phantomjs /usr/local/bin/phantomjs &&\
  rm -rf /tmp/*

WORKDIR /app

# clone spacedeck repo

RUN git clone https://github.com/zuk38/spacedeck.git
RUN mv spacedeck/* .
RUN mv spacedeck/.git .

# install node package

RUN npm install
RUN npm install -g --save-dev gulp
RUN gulp styles

# start app

CMD ["sh", "-c", "git pull; node spacedeck.js"]
EXPOSE 9666
