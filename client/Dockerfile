# =========================
# Build stage
# =========================

FROM node:26-slim AS build

WORKDIR /app

ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Copy package files first for Docker caching
COPY package*.json ./

RUN npm ci

# Copy source code
COPY . .

# Build React application
RUN echo "VITE_API_BASE_URL=$VITE_API_BASE_URL" 
RUN npm run build


# =========================
# Runtime stage
# =========================

FROM nginx:alpine AS runtime

# Remove default nginx content
RUN rm -rf /usr/share/nginx/html/*

# Copy React build
COPY --from=build /app/dist /usr/share/nginx/html

# Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]