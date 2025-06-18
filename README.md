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
**CMD test cho prolog**
<br />
```txt
swipl -q -f expertTest.pl -g "query_from_input([feature(memory,'4'),feature(brands,'SAMSUNG'),feature(rating,'4'),feature(storage,'64')])" -t halt
```
<br />
<br />
**Kết quả mong đợi: Top score 3.77, clusters [0,7,1,2,5,4]** <br />

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
