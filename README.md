## Swarm 基礎知識

### 架構
Swarm 是由 Node 所組成，Node 可以是 Manager 或是 Worker。
重點是一個 Swarm 需要一個 Manager 才能分配工作，其他是 Worker；
而 Manager 自己也可以執行 Container，不一定要是 Worker。

### 啟動一個 Swarm
首先要先啟動一個 Swarm 設定 Manager 是誰？
他的 `IP` 參數設置給 &lt;Manager_IP&gt;，
這段程式碼要在 Manager 電腦上執行。
```sh

docker swarm init --advertise-addr <Manager_IP>
```
之後取得作為 Worker 加入的 `token`
```sh

docker swarm join-token worker
```

### 將電腦加入為 Worker
從上述的 `token` 當作 &lt;TOKEN&gt 與 `IP` 當作 &lt;Manager_IP&gt;
輸入以加入 Swarm 作為工作電腦
```sh

docker swarm join --token <TOKEN> <Manager_IP>:2377
```