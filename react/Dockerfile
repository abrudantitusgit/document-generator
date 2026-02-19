# Use Node.js 18 Alpine as base image
FROM node:18-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies with legacy peer deps to resolve conflicts
RUN npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Expose port 3000 (React development server default)
EXPOSE 3000

# Set environment variables to allow external connections
ENV HOST=0.0.0.0
ENV PORT=3000
ENV BROWSER=none

# Start the React development server
CMD ["npm", "start"]
