#!/bin/bash

################################################################################
# upgrade.sh - Script to update and redeploy the MarinAPI from GitHub
#
# This script performs the following steps:
#   1. Navigates to the project directory
#   2. Pulls the latest changes from the main branch of GitHub
#   3. Cleans up unused containers, volumes, and orphans
#   4. Rebuilds the Docker image with --no-cache
#   5. Restarts the Docker container in detached mode
#
# Usage:
#   chmod +x upgrade.sh     # Make it executable
#   ./upgrade.sh            # Run the upgrade process
################################################################################

# STEP 1: Set variables
PROJECT_DIR="$HOME/marinapi"
DOCKER_DIR="$PROJECT_DIR/src"
BRANCH="main"

echo "🔄 Starting upgrade for MarinAPI..."

# STEP 2: Navigate to project directory
echo "📁 Navigating to $PROJECT_DIR"
cd "$PROJECT_DIR" || { echo "❌ Failed to enter project directory: $PROJECT_DIR"; exit 1; }

# STEP 3: Ensure we're on the correct Git branch
echo "🌿 Switching to branch: $BRANCH"
git checkout $BRANCH

# STEP 4: Pull latest changes from GitHub
echo "⬇️ Pulling latest changes from origin/$BRANCH"
git fetch origin
git reset --hard origin/$BRANCH
git clean -fd

# STEP 5: Navigate to Docker Compose directory
echo "📁 Navigating to Docker directory: $DOCKER_DIR"
cd "$DOCKER_DIR" || { echo "❌ Failed to enter Docker directory: $DOCKER_DIR"; exit 1; }

# STEP 6: Shut down running containers and clean up
echo "🧹 Stopping and cleaning up existing Docker containers"
docker compose down --volumes --remove-orphans

# STEP 7: Rebuild Docker image from scratch
echo "🔨 Rebuilding Docker image (no cache)"
docker compose build --no-cache

# STEP 8: Start container in detached mode
echo "🚀 Starting Docker container"
docker compose up -d

echo "✅ Upgrade complete. Your MarinAPI is now running the latest version."
