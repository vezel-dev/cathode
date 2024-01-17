const int Volume = 3;

await OutAsync(
    new ControlBuilder()
        .PlayNotes(Volume, duration: 2, [1, 3, 5])
        .PlayNotes(Volume, duration: 8, [6, 10, 8, 11])
        .PlayNotes(Volume, duration: 4, [10, 11, 13, 10])
        .PlayNotes(Volume, duration: 8, [8, 11])
        .PlayNotes(Volume, duration: 4, [10, 11, 13, 10, 11, 13, 15, 17])
        .PlayNotes(Volume, duration: 8, [18, 17, 18]));
