# 1. Config
   - Appplicatio nmust not used hard-coded IP addressess or Port numbers; these configuration must be read from App.config file at runtime to allow configuration multiple computers withou recompiling the solution
   - Dyanmic file threshold ```maxFileSize``` limit triggers graceful shutdown must be as well implemented on App.config

# 2 Constants Handling
   - Majority to all magic number shall be eliminated and constants name should be declared Constant.cs (utils/Constants.cs)
     
# 3. Networking and Concurrency
   - Use of Concurrent Client Handling: Server must use ```Task.Run``` or an equivalen async patter tn ohandle multiple simultanenous client connections without blocking the main listner loop.
   - Data packet integrity: When client sends data, the server must correctly decode before logging into the log file.
     
# 4. File I/O
   - Asynchronous logging: All file write operations must use asnychrnous methods (e.g. ```WriteLineAsync```) (Week 2 class).
   - Centralized logging: All communication from all clients must be **appended** to a signle log.txt file in chronoglogical order.
     
# 5. Requirement 3
   *When the file reaches a certain size, it must notify the clients so they can stop sending 
    data. It must then stop all local tasks. **These stops must be graceful.***
   - Once LogManager detects the file has reached the ```maxFileSize```, it switch global bool "isRunning" flag to false
   - Upon shutdown, the server must stop the ```TcpListener``` and close all active TcpClient connections properly before exiting the process
     
# 6. Requirement 4 (Performance Metrics)\
   *Experiment with the performance of your solution. You must come up with your own 
metrics and one of them MUST be some form of valid time measurement.*
   - Client must utilize the ```System.Diagnostics.Stopwatch``` class to measure the **Round Trip Time** (RTT) of dat packet in miliseconds (ms)
   - Implement metric comparison where the report must compare the performance of synchronous vs asynchronous writing justify chosen solution
