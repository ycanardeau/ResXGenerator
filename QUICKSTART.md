# Options

|Option|Global|Accepted values          |Compatibilty  |
|------|------|-------------------------|--------------|
|GenerationType|ResXGenerator_GenerationType|- **ResourceManager**</br>- CodeGeneration</br>- StringLocalizer</br>- SameAsOuter|All|
|PublicClass|ResXGenerator_PublicClass|- true<br>- **false**|All|
|StaticClass|ResXGenerator_StaticClass|- **true**<br>- false|All|
|PartialClass|ResXGenerator_PartialClass|- true<br>- **false**|All except StringLocalizer|
|StaticMembers|ResXGenerator_StaticMembers|- **true**<br>- false|All except StringLocalizer|
|NullForgivingOperators|ResXGenerator_NullForgivingOperators|- true<br>- **false**|All except StringLocalizer|
|InnerClassVisibility|ResXGenerator_InnerClassVisibility|-public<br>- internal<br>- private<br>- protected<br>-  sameasouter<br>- **notgenerated**|All except StringLocalizer|
|InnerClassName|ResXGenerator_InnerClassName|Any valid C# identifier|
|InnerClassInstanceName|ResXGenerator_InnerClassInstanceName|Any valid C# identifier|
|ClassNamePostfix|ResXGenerator_ClassNamePostfix|Any valid C# identifier|All except StringLocalizer|
|GenerateCode*|ResXGenerator_GenerateCode|- true<br>- **false**|All except StringLocalizer|
|CustomToolNamespace|ResXGenerator_CustomToolNamespace|Any valid C# namespace|All except StringLocalizer|
|SkipFile| |- true<br>- **false**|All|

\* *Could be replaced by the option GenerationType with the value CodeGeneration*
