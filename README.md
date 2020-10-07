![Logo](src/icon.png)

# HashAlgorithms [![Build status](https://github.com/StephenCleary/HashAlgorithms/workflows/Build/badge.svg)](https://github.com/StephenCleary/HashAlgorithms/actions?query=workflow%3ABuild) [![codecov](https://codecov.io/gh/StephenCleary/HashAlgorithms/branch/master/graph/badge.svg)](https://codecov.io/gh/StephenCleary/HashAlgorithms) [![NuGet version](https://badge.fury.io/nu/Nito.HashAlgorithms.svg)](https://www.nuget.org/packages/Nito.HashAlgorithms) [![API docs](https://img.shields.io/badge/API-dotnetapis-blue.svg)](http://dotnetapis.com/pkg/Nito.HashAlgorithms)

Hash algorithm implementations for .NET, including CRC (CRC32 and CRC16).

All hash algorithms support streaming (hashing a block at a time) and have full support for `Span<T>` to prevent unnecessary memory allocation. They also have excellent downlevel platform support (`netstandard1.3`).

## CRC32

`CRC32` is a generic 32-bit CRC implementation, patterned after [Boost's CRC library](https://www.boost.org/doc/libs/1_66_0/libs/crc/).

### Creating a CRC32

You can pass a `CRC32.Definition` instance into the constructor to control all the CRC algorithm parameters.

```C#
var defaultHasher = new CRC32(); // uses CRC32.Definition.Default
var bzipHasher = new CRC32(CRC32.Definition.BZip2);
var customHasher = new CRC32(new CRC32.Definition
{
    TruncatedPolynomial = 0x1EDC6F41,
    Initializer = 0xFFFFFFFF,
    FinalXorValue = 0xFFFFFFFF,
    ReverseDataBytes = true,
    ReverseResultBeforeFinalXor = true,
});
```

Once created, the `CRC32` instance can be used like any other `HashAlgorithm` instance.

### Implementations

Built-in implementations include:

- `Default` - an older CRC-32 defined by the old IEEE standard. Used by Ethernet, zip, PNG, and many other systems. Known as `"CRC-32"`, `"CRC-32/ADCCP"`, and `"PKZIP"`. This is the default CRC-32 definition because it is the most common; however, there are better polynomials if you are defining a new protocol/format.
- `BZip2` - a CRC-32 used by BZIP2. Known as `"CRC-32/BZIP2"` and `"B-CRC-32"`.
- `Castagnoli` - a modern CRC-32 defined in [RFC 3720 (iSCSI)](https://tools.ietf.org/html/rfc3720). Known as `"CRC-32C"`, `"CRC-32/ISCSI"`, and `"CRC-32/CASTAGNOLI"`.
- `Mpeg2` - a CRC-32 used by the MPEG-2 standard. Known as `"CRC-32/MPEG-2"`.
- `Posix` - a CRC-32 used by the POSIX `chksum` command; note that the `chksum` command-line program appends the file length to the contents unless it is empty. Known as `"CRC-32/POSIX"` and `"CKSUM"`.
- `Aixm` - a CRC-32 used in the Aeronautical Information eXchange Model. Known as `"CRC-32Q"`.
- `Xfer` - a very old CRC-32, appearing in [Numerical Recipes in C](https://amzn.to/3d725VX). Known as `"XFER"`.

## CRC16

`CRC16` is a generic 16-bit CRC implementation, patterned after [Boost's CRC library](https://www.boost.org/doc/libs/1_66_0/libs/crc/).

### Creating a CRC16

You can pass a `CRC16.Definition` instance into the constructor to control all the CRC algorithm parameters.

```C#
var defaultHasher = new CRC16(); // uses CRC16.Definition.Default
var ccittHasher = new CRC16(CRC16.Definition.Ccitt);
var customHasher = new CRC16(new CRC16.Definition
{
    TruncatedPolynomial = 0x1021,
    Initializer = 0xFFFF,
    FinalXorValue = 0xFFFF,
    ReverseDataBytes = true,
    ReverseResultBeforeFinalXor = true,
});
```

Once created, the `CRC16` instance can be used like any other `HashAlgorithm` instance.

### Implementations

Built-in implementations include:

- `Default` - a common CRC-16, used by ARC and LHA.
- `CcittFalse` - a CRC-16 used by floppy disk formats, commonly misidentified as CCITT.
- `Ccitt` - a CRC-16 used by Kermit. Known as `"CCITT"`. Appears in [Numerical Recipes in C](https://amzn.to/3d725VX).
- `XModem` - a CRC-16 used by XMODEM and ZMODEM. Appears in [Numerical Recipes in C](https://amzn.to/3d725VX).
- `X25` - a CRC-16 used by X.25, V.42, T.30, and [RFC 1171 (PPP)](https://tools.ietf.org/html/rfc1171). Appears in [Numerical Recipes in C](https://amzn.to/3d725VX).

## HashAlgorithmBase

The `Nito.HashAlgorithm.Core` NuGet library has a `HashAlgorithmBase` type that provides an easier way to define hash algorithms across the widest possible number of platforms (`netstandard1.3` - `netstandard2.1`). It pulls in `System.Memory` on platforms that require it to provide support for `Span<T>`.

To use `HashAlgorithmBase`, your hash algorithm implementation should pass the size of the hash (in bits) to the `HashAlgorithmBase` constructor. `HashAlgorithmBase` will set `HashSizeValue` or override `HashSize` appropriately, depending on platform.

Then implement these two methods:

```C#
protected void DoHashCore(ReadOnlySpan<byte> source);
protected void DoHashFinal(Span<byte> destination);
```

The `DoHashCore` method should hash the bytes in `source`. The `DoHashFinal` method should save the hash result into `destination` (hint: `BinaryPrimitives` is useful when implementing this method).

That's it; the `HashAlgorithmBase` takes care of implementing the `HashAlgorithm` methods, including overrides for efficient `Span<T>`-based hashing.
