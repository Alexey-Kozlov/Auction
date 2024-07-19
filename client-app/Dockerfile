FROM node:18-alpine AS deps
WORKDIR /app

COPY client-app/package.json .
RUN  npm install --omit-dev

FROM node:18-alpine AS builder
WORKDIR /app
COPY client-app/. .

RUN npm run build

FROM nginx

ENV NODE_ENV=production

COPY --from=builder /app/build /usr/share/nginx/html
COPY client-app/default.conf /etc/nginx/conf.d/
