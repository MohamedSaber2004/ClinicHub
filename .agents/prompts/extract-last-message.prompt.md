---
description: "Analyze the last message in the conversation history and return its content type (media or text) along with the message content"
argument-hint: "Extract and identify: media (image, audio, video, file) vs text content"
agent: "agent"
---

Analyze the last message from the conversation history and perform the following:

1. **Identify Content Type**: Determine whether the last message contains:
   - **Media**: Image, audio, video, or file attachment
   - **Text**: Message content or text

2. **Extract Content**: 
   - If media: Return the media type and filename/description
   - If text: Return the full text content

3. **Format Response**:
   ```
   Content Type: [MEDIA | TEXT]
   Type Details: [image/png, audio/mp3, text, etc.]
   Content: [extracted message or media info]
   ```

4. **Handle Edge Cases**:
   - If the message has both media and text, indicate both
   - If no previous message exists, report "No previous message"
   - Preserve the exact formatting and content of text messages
