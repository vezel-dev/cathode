// SPDX-License-Identifier: 0BSD

#pragma once

#include <assert.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#define atomic _Atomic
#define nonnull _Nonnull
#define nullable _Nullable

#if defined(ZIG_OS_WINDOWS)
#   define CATHODE_API [[gnu::dllexport]]
#else
#   define CATHODE_API [[gnu::visibility("default")]]
#endif
