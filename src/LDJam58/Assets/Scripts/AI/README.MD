# AI Tools

This folder contains AI-related utilities and tools for the Unity Editor.

## Project Secrets

A secure key-value store for managing API keys and other secrets in your Unity project.

### Access

Open via `Tools > AI > Project Secrets`

### Features

- Store API keys and secrets locally (not committed to version control)
- Add, edit, and delete secrets
- Values are masked in the UI for security
- Quick setup button for common secrets like Replicate API

## Flux Image Generator

A Unity Editor window for generating images using Flux Pro 1.1 via the Replicate API.

### Setup

1. Add your Replicate API key:
   - Open `Tools > AI > Project Secrets`
   - Click "Add Replicate API Key" or manually add key `REPLICATE_API_TOKEN`
   - Paste your API token
   - Alternatively, set the `REPLICATE_API_TOKEN` environment variable
2. (Optional) Create a `FluxPromptSettings` ScriptableObject via `Assets > Create > AI > Flux Prompt Settings`
3. Configure the base prompt in the ScriptableObject for style guidelines
4. Open the generator window via `Tools > AI > Flux Image Generator`

### Usage

- (Optional) Assign your FluxPromptSettings ScriptableObject for base style prompt
- Enter specific prompt details (required) - what you want to generate
- Choose 1 or 4 images from the dropdown
- Click "Generate Images"
- Images will be saved to `Assets/Generated/` as JPG files (quality 80)

### Prompt Behavior

- If only specific prompt is provided: Uses that prompt alone
- If both are provided: `"{specific prompt}, {base prompt}"`
- The specific prompt comes first for emphasis, base prompt provides style guidelines


