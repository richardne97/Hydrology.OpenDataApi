## 水利署水文資料開放API 客戶端開發範例

1. 使用語言 c#
2. 環境版本 .net framework 4.6.2
3. 開發工具 [Visual Studio 2019 community](https://visualstudio.microsoft.com/vs/community/)

**使用說明**
1. 此 Depository 包括三個 Project，分別為 Hydrology.OpenDataApi.Model, Hydrology.OpenDataApi.Client 以及 Hydrology.OpenDataApi.Client.Example，以下簡稱Model, Clinet, Example
2. Model為使用 OpenDataApi Http Get/Post 時Request以及Response的資料JSON模型，以class定義並透過JsonConverter轉換為JOSN格式進行傳輸。
3. Client包含了兩個主要物件，分別為OAuth2Client及HodApiClient，OAuth2Client支援ClientCredential認證方式之客戶端，只要給定由OpenDataApi平台申請帳號所取得ClinetId, ClientSecret，就可透過此物件取得Token，進行API呼叫。HodApiClient則透過Http Get/Post以及OAuth2Client取得的Token，呼叫OpenDataApi平台提供之WebApi取得水文資料。
4. Example為可執行之.net framework console程式，引用Model及Client中的物件，提供取得各種水文資料的方法呼叫範例。

