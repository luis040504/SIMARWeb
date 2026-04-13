FROM node:20-alpine

WORKDIR /app

COPY package*.json ./

RUN npm ci --only=production

COPY . .

RUN mkdir -p uploads/manifiestos

EXPOSE 8007

CMD ["node", "src/index.js"]
