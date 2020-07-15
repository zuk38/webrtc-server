FROM node:10-alpine3.10

WORKDIR /app

# clone webrtc-server repo

RUN apk add git
RUN git clone https://github.com/zuk38/webrtc-server.git
RUN mv webrtc-server/* .
RUN mv webrtc-server/.git .

# install node package

RUN npm install

# start app

CMD ["sh", "-c", "git pull; node server.js"]
EXPOSE 8444
