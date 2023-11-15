await OutAsync(
    new ControlBuilder()
        .PlayNotes(3, 2, [1, 3, 5])
        .PlayNotes(3, 8, [6, 10, 8, 11])
        .PlayNotes(3, 4, [10, 11, 13, 10])
        .PlayNotes(3, 8, [8, 11])
        .PlayNotes(3, 4, [10, 11, 13, 10, 11, 13, 15, 17])
        .PlayNotes(3, 8, [18, 17, 18]));
