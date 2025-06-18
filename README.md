# Bước 1
**Clone link GitHub: git clone -b Backend https://github.com/phananh85327/ExpertSmartphone** <br />

# Bước 2
**Setup Python env (3.9) trong thư mục /Python** <br />

# Bước 3
**Setup DB: Chạy /Bak/Product.sql script để tạo database và table** <br />

# Bước 4
**Setup DB connection string: Thay đổi connection string tại /Config.json** <br />

# Bước 5
**Install SWI: https://www.swi-prolog.org/Download.html** <br />

# Bước 6
**Thay đổi SWI trỏ đến địa chỉ local tại /Data/Constants.cs cho biến SWI_FILE_PATH** <br />

# Bước 7
**Import Sales_completed.csv bằng HttpPost("Import")** <br />
**Dataset được tạo kết hợp từ 3 nguồn dataset từ Kaggle** <br />
**Link 1: https://www.kaggle.com/datasets/yaminh/smartphone-sale-dataset/data** <br />
**Link 2: https://www.kaggle.com/code/azzayahia/phone-sales-data-analysis/input** <br />
**Link 3: https://www.kaggle.com/code/ginelledsouza/phone-analysis/input** <br />

# Bước 8
**Chạy HttpPost("Build") để gom cụm dữ liệu và generate expert.pl file** <br />

# Thực nghiệm cho prolog
**CMD test cho prolog (kết quả mong đợi: Top score 6.66, clusters [8])**
<br />
```txt
swipl -q -f "C:\00 - 00\00 - Projects\ExpertSmartphone\Backend\bin\Debug\net8.0\..\..\..\expertTest.pl" -g "query_from_input([feature(brands,'Apple'),feature(colors,'Silver'),feature(memory,'8'),feature(storage,'256'),feature(rating,'5'),feature(sellingprice,'50000'),feature(discountpercentage,'0'),feature(os,'IOS'),feature(sellersamount,'100'),feature(screensize,'6'),feature(batterysize,'5000'),feature(reviews,'300')])" -t halt
```
<br />

# Thực nghiệm khi insert / update dữ liệu
**Json body test cho HttpPost("Insert") / HttpPut("Update/{id}")**
```json
{
  "brands": "Apple",
  "models": "iPhone 16 Pro Max",
  "colors": "Silver",
  "memory": 8,
  "storage": 256,
  "camera": true,
  "rating": 5,
  "originalPrice": 100000,
  "mobile": "Apple iPhone 16 Pro Max",
  "discountPercentage": 0,
  "os": "IOS",
  "sellersAmount": 20,
  "screenSize": 6.7,
  "batterySize": 5000,
  "reviews": 300
}
```
<br />

# Thực nghiệm khi lấy đề xuất khuyến nghị
**Json body test cho HttpPost("GetExpertResults")**
```json
{
  "brands": "SAMSUNG",
  "memory": 4,
  "storage": 64,
  "rating": 4
}
```
