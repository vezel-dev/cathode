Terminal.OutLine("Listening for resize events.");
Terminal.OutLine();

Terminal.Resized += size => Terminal.OutLine($"Width = {size.Width}, Height = {size.Height}");

await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
