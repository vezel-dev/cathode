#include "driver.h"

void cathode_initialize(void)
{
    // Do our best to start in cooked mode.
    cathode_set_mode(false, false);
}
