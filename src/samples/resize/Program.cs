await OutLineAsync("Listening for resize events.");
await OutLineAsync();

Resized += size => OutLine($"Width = {size.Width}, Height = {size.Height}");

await Task.Delay(Timeout.Infinite);
