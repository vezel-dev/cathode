await OutAsync(
    new ControlBuilder()
        .PlayNotes(3, 2, stackalloc[] { 1, 3, 5 })
        .PlayNotes(3, 8, stackalloc[] { 6, 10, 8, 11 })
        .PlayNotes(3, 4, stackalloc[] { 10, 11, 13, 10 })
        .PlayNotes(3, 8, stackalloc[] { 8, 11 })
        .PlayNotes(3, 4, stackalloc[] { 10, 11, 13, 10, 11, 13, 15, 17 })
        .PlayNotes(3, 8, stackalloc[] { 18, 17, 18 }));
