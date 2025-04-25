# Path to your local models directory
$modelsFolder = "C:\full\path\to\models"
# Your quantized model filename
$modelFile   = "ggml-gpt4all-lora-q4_0.bin"

Write-Host "Starting LocalAI Docker container on http://localhost:8080 …"

docker run --rm -p 8080:8080 `
    -v "$modelsFolder:/models" `
    ghcr.io/go-skynet/localai:latest `
    --model-path "/models/$modelFile"
