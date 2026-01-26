// Canvas drawing functionality
let isDrawing = false;
let context = null;
let autoUpdateVisualisation = false;
let drawingCanvasId = null;

export function initializeCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    drawingCanvasId = canvasId;
    context = canvas.getContext('2d');
    context.lineWidth = 18;
    context.lineCap = 'round';
    context.lineJoin = 'round';
    context.strokeStyle = '#000000';
    // Enable smoothing for blur like MNIST
    context.imageSmoothingEnabled = true;

    // Mouse events
    canvas.addEventListener('mousedown', startDrawing);
    canvas.addEventListener('mousemove', draw);
    canvas.addEventListener('mouseup', stopDrawing);
    canvas.addEventListener('mouseout', stopDrawing);

    // Touch events for mobile
    canvas.addEventListener('touchstart', handleTouchStart);
    canvas.addEventListener('touchmove', handleTouchMove);
    canvas.addEventListener('touchend', stopDrawing);

    // Clear canvas initially
    clearCanvas(canvasId);
}

export function enableAutoVisualisation(enabled) {
    autoUpdateVisualisation = enabled;
    updateVisualisationIfEnabled();
}

function updateVisualisationIfEnabled() {
    if (autoUpdateVisualisation && drawingCanvasId) {
        const pixelData = processImageTo28x28(drawingCanvasId);
        visualiseInputLayer('visualizationCanvas', pixelData);
    }
}

function startDrawing(e) {
    isDrawing = true;
    const rect = e.target.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    context.beginPath();
    context.moveTo(x, y);
}

function draw(e) {
    if (!isDrawing) return;
    const rect = e.target.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    context.lineTo(x, y);
    context.stroke();
    updateVisualisationIfEnabled();
}

function stopDrawing() {
    isDrawing = false;
    updateVisualisationIfEnabled();
}

function handleTouchStart(e) {
    e.preventDefault();
    const touch = e.touches[0];
    const rect = e.target.getBoundingClientRect();
    const x = touch.clientX - rect.left;
    const y = touch.clientY - rect.top;
    isDrawing = true;
    context.beginPath();
    context.moveTo(x, y);
}

function handleTouchMove(e) {
    e.preventDefault();
    if (!isDrawing) return;
    const touch = e.touches[0];
    const rect = e.target.getBoundingClientRect();
    const x = touch.clientX - rect.left;
    const y = touch.clientY - rect.top;
    context.lineTo(x, y);
    context.stroke();
    updateVisualisationIfEnabled();
}

export function clearCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    updateVisualisationIfEnabled();
}

export function getCanvasImageData(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return '';
    
    return canvas.toDataURL('image/png');
}

export function processImageTo28x28(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return [];

    const ctx = canvas.getContext('2d');
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const pixels = imageData.data;
    
    // Find bounding box of non-white pixels
    let minX = canvas.width, minY = canvas.height, maxX = 0, maxY = 0;
    let hasContent = false;
    
    for (let y = 0; y < canvas.height; y++) {
        for (let x = 0; x < canvas.width; x++) {
            const idx = (y * canvas.width + x) * 4;
            const r = pixels[idx];
            const g = pixels[idx + 1];
            const b = pixels[idx + 2];
            
            // Check if pixel is not white (has drawing)
            if (r < 250 || g < 250 || b < 250) {
                hasContent = true;
                minX = Math.min(minX, x);
                minY = Math.min(minY, y);
                maxX = Math.max(maxX, x);
                maxY = Math.max(maxY, y);
            }
        }
    }
    
    // If no content, return empty grid
    if (!hasContent) {
        return new Array(784).fill(-0.5);
    }
    
    // Calculate bounding box dimensions with some padding
    const padding = 10;
    minX = Math.max(0, minX - padding);
    minY = Math.max(0, minY - padding);
    maxX = Math.min(canvas.width - 1, maxX + padding);
    maxY = Math.min(canvas.height - 1, maxY + padding);
    
    const boundingWidth = maxX - minX + 1;
    const boundingHeight = maxY - minY + 1;
    
    // Create a temporary canvas for the cropped content
    const croppedCanvas = document.createElement('canvas');
    croppedCanvas.width = boundingWidth;
    croppedCanvas.height = boundingHeight;
    const croppedCtx = croppedCanvas.getContext('2d');
    croppedCtx.fillStyle = '#FFFFFF';
    croppedCtx.fillRect(0, 0, boundingWidth, boundingHeight);
    croppedCtx.drawImage(canvas, minX, minY, boundingWidth, boundingHeight, 0, 0, boundingWidth, boundingHeight);
    
    // Create 20x20 canvas and resize cropped content to fit
    const resizedCanvas = document.createElement('canvas');
    resizedCanvas.width = 20;
    resizedCanvas.height = 20;
    const resizedCtx = resizedCanvas.getContext('2d');
    resizedCtx.fillStyle = '#FFFFFF';
    resizedCtx.fillRect(0, 0, 20, 20);
    
    // Calculate scaling to fit in 20x20 while maintaining aspect ratio
    const scale = Math.min(20 / boundingWidth, 20 / boundingHeight);
    const scaledWidth = boundingWidth * scale;
    const scaledHeight = boundingHeight * scale;
    const offsetX = (20 - scaledWidth) / 2;
    const offsetY = (20 - scaledHeight) / 2;
    
    resizedCtx.drawImage(croppedCanvas, 0, 0, boundingWidth, boundingHeight, 
                         offsetX, offsetY, scaledWidth, scaledHeight);
    
    // Create final 28x28 canvas with 20x20 centered
    const finalCanvas = document.createElement('canvas');
    finalCanvas.width = 28;
    finalCanvas.height = 28;
    const finalCtx = finalCanvas.getContext('2d');
    finalCtx.fillStyle = '#FFFFFF';
    finalCtx.fillRect(0, 0, 28, 28);
    
    // Center the 20x20 image in 28x28 (4 pixels padding on each side)
    finalCtx.drawImage(resizedCanvas, 0, 0, 20, 20, 4, 4, 20, 20);
    
    // Get the final image data
    const finalImageData = finalCtx.getImageData(0, 0, 28, 28);
    const finalPixels = finalImageData.data;
    
    // Convert RGBA to grayscale normalized values
    const grayscalePixels = [];
    for (let i = 0; i < finalPixels.length; i += 4) {
        const r = finalPixels[i];
        const g = finalPixels[i + 1];
        const b = finalPixels[i + 2];
        
        // Average RGB to get grayscale
        const gray = (r + g + b) / 3.0;
        
        // Normalize to -0.5 to 0.5 (same as training data)
        // Canvas: white (255) should map to MNIST white (0) -> -0.5
        // Canvas: black (0) should map to MNIST black (255) -> 0.5
        // So we need to invert: (255 - gray) / 255.0 - 0.5
        const inverted = 255.0 - gray;
        const normalized = (inverted / 255.0) - 0.5;
        
        grayscalePixels.push(normalized);
    }

    return grayscalePixels;
}

export function visualiseInputLayer(canvasId, pixelData) {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !pixelData || pixelData.length !== 784) return;
    
    const ctx = canvas.getContext('2d');
    const cellSize = canvas.width / 28;
    
    // Clear canvas with dark background
    ctx.fillStyle = '#1a1a1a';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Draw each pixel as a colored cell
    for (let y = 0; y < 28; y++) {
        for (let x = 0; x < 28; x++) {
            const idx = y * 28 + x;
            const value = pixelData[idx];
            
            // Value ranges from -0.5 (background) to 0.5 (ink)
            // Map to colors: negative (blue) -> zero (black) -> positive (red)
            let r, g, b;
            
            if (value > 0) {
                // Positive values (ink) - red
                const intensity = Math.min(value * 2, 1.0); // Scale 0 to 0.5 -> 0 to 1
                r = Math.floor(255 * intensity);
                g = 0;
                b = 0;
            } else {
                // Negative values (background) - blue
                const intensity = Math.min(Math.abs(value) * 2, 1.0); // Scale 0 to -0.5 -> 0 to 1
                r = 0;
                g = 0;
                b = Math.floor(255 * intensity);
            }
            
            ctx.fillStyle = `rgb(${r}, ${g}, ${b})`;
            ctx.fillRect(x * cellSize, y * cellSize, cellSize, cellSize);
            
            // Optional: draw grid lines
            ctx.strokeStyle = '#333333';
            ctx.lineWidth = 0.5;
            ctx.strokeRect(x * cellSize, y * cellSize, cellSize, cellSize);
        }
    }
    
    // Optional: display min/max values as overlay
    const min = Math.min(...pixelData);
    const max = Math.max(...pixelData);
    
    ctx.fillStyle = 'white';
    ctx.font = '12px monospace';
    ctx.fillText(`Min: ${min.toFixed(3)}`, 5, 15);
    ctx.fillText(`Max: ${max.toFixed(3)}`, 5, 30);
}

export function visualiseOutputNeurons(canvasId, outputValues) {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !outputValues || outputValues.length !== 10) return;
    
    const ctx = canvas.getContext('2d');
    const width = canvas.width;
    const height = canvas.height;
    
    // Clear canvas with dark background
    ctx.fillStyle = '#1a1a1a';
    ctx.fillRect(0, 0, width, height);
    
    // Calculate dimensions for each output bar
    const barHeight = 30;
    const barMaxWidth = width - 100;
    const startX = 50;
    const spacing = (height - 20) / 10;
    
    // Find max value for scaling
    const maxValue = Math.max(...outputValues);
    
    // Draw each output neuron as a horizontal bar
    for (let i = 0; i < 10; i++) {
        const y = 10 + i * spacing + spacing / 2 - barHeight / 2;
        const value = outputValues[i];
        const barWidth = (value / maxValue) * barMaxWidth;
        
        // Draw label (digit number)
        ctx.fillStyle = 'white';
        ctx.font = 'bold 20px monospace';
        ctx.textAlign = 'right';
        ctx.textBaseline = 'middle';
        ctx.fillText(i.toString(), startX - 10, y + barHeight / 2);
        
        // Draw background bar
        ctx.fillStyle = '#333333';
        ctx.fillRect(startX, y, barMaxWidth, barHeight);
        
        // Draw value bar with gradient
        if (value > 0.01) {
            const gradient = ctx.createLinearGradient(startX, 0, startX + barWidth, 0);
            
            // Color based on relative strength
            const intensity = value / maxValue;
            if (intensity > 0.8) {
                gradient.addColorStop(0, '#00ff00');  // Green for winner
                gradient.addColorStop(1, '#00aa00');
            } else if (intensity > 0.5) {
                gradient.addColorStop(0, '#ffff00');  // Yellow for strong
                gradient.addColorStop(1, '#aaaa00');
            } else {
                gradient.addColorStop(0, '#ff6600');  // Orange for weak
                gradient.addColorStop(1, '#aa4400');
            }
            
            ctx.fillStyle = gradient;
            ctx.fillRect(startX, y, barWidth, barHeight);
        }
        
        // Draw value text
        ctx.fillStyle = 'white';
        ctx.font = '14px monospace';
        ctx.textAlign = 'left';
        ctx.fillText(value.toFixed(4), startX + barMaxWidth + 10, y + barHeight / 2);
        
        // Highlight the winner
        if (value === maxValue && maxValue > 0.01) {
            ctx.strokeStyle = '#00ff00';
            ctx.lineWidth = 3;
            ctx.strokeRect(startX - 5, y - 2, barMaxWidth + 10, barHeight + 4);
        }
    }
    
    // Draw title
    ctx.fillStyle = 'white';
    ctx.font = 'bold 14px monospace';
    ctx.textAlign = 'center';
    ctx.fillText('Digit Confidence', width / 2, height - 5);
}
