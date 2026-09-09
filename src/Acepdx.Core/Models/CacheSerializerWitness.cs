using PolyType;

namespace Acepdx.Core.Models;

[GenerateShapeFor<LicenseList>]
[GenerateShapeFor<LicenseListEntry>]
[GenerateShapeFor<License>]
[GenerateShapeFor<CrossReference>]
[GenerateShapeFor<CachedData<LicenseList>>]
internal partial class CacheSerializerWitness;