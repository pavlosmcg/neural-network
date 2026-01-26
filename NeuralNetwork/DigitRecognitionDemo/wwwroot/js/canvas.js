// Canvas drawing functionality
let isDrawing = false;
let context = null;

export function initializeCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    context = canvas.getContext('2d');
    context.lineWidth = 15;
    context.lineCap = 'round';
    context.strokeStyle = '#000000';

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
}

function stopDrawing() {
    isDrawing = false;
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
}

export function clearCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
}

export function getCanvasImageData(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return '';
    
    return canvas.toDataURL('image/png');
}

export function processImageTo28x28(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return [];

    // Create a temporary canvas for resizing
    const tempCanvas = document.createElement('canvas');
    tempCanvas.width = 28;
    tempCanvas.height = 28;
    const tempCtx = tempCanvas.getContext('2d');

    // Draw the original canvas onto the temp canvas (scaled down)
    tempCtx.fillStyle = '#FFFFFF';
    tempCtx.fillRect(0, 0, 28, 28);
    tempCtx.drawImage(canvas, 0, 0, canvas.width, canvas.height, 0, 0, 28, 28);

    // Get the image data
    const imageData = tempCtx.getImageData(0, 0, 28, 28);
    const pixels = imageData.data;

    // Convert RGBA to grayscale normalized values (0-1)
    // Background is white (255), drawing is black (0)
    // We invert so drawing becomes 1.0 and background becomes 0.0
    const grayscalePixels = [];
    for (let i = 0; i < pixels.length; i += 4) {
        const r = pixels[i];
        const g = pixels[i + 1];
        const b = pixels[i + 2];
        
        // Average RGB to get grayscale
        const gray = (r + g + b) / 3.0;
        
        // Invert (white background -> 0, black drawing -> 1) and normalize to -0.5 to 0.5
        const normalized = 0.5 - (gray / 255.0);
        
        grayscalePixels.push(normalized);
    }

    return grayscalePixels;
}
