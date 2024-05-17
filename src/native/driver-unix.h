// SPDX-License-Identifier: 0BSD

#pragma once

#include "driver.h"

CATHODE_API void cathode_poll(bool write, const int *nonnull fds, bool *nullable results, int count);
