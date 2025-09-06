FROM node:18-alpine as build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Use npm install instead of npm ci for better compatibility
RUN npm install

# Copy source code
COPY . .

# Build the React app
RUN npm run build

# Production stage with nginx
FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]