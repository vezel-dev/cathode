#pragma once

#include "driver.h"

CATHODE_API void cathode_poll(bool write, const size_t *nonnull handles, bool *nullable results, int count);
