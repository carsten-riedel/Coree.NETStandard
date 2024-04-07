## Coree.NETStandard.Extensions.Bool

<!-- comment -->

Example
```
using Coree.NETStandard.Extensions.Bool

bool? newFeatureEnabled = FeatureFlagManager.GetFeatureStatus("NewFeature");

if (newFeatureEnabled.IsTrue())
{
    // clearly true
}
else (!newFeatureEnabled.IsTrue())
{
    // could be false or null
}
```
