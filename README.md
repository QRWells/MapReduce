# MapReduce

A simple MapReduce implementation in C#. (Work in progress)

## Build

To build the project, you need to have the .NET Core SDK installed. Then, run the following command:

```bash
./build.sh
```
or 

```powershell
./build.ps1
```

After the build is complete, the binaries will be in the `output` folder.

## Run

### Coordinator

```bash
./MapReduce.exe coordinator -n <number to reduce> -f <input files>
```
### Worker

```bash
./MapReduce.exe worker -h <coordinator host> -p <coordinator port> -w <lib containing worker implementation>
```

## License

This project is licensed under the terms of the MIT license. See the (LICENSE)[LICENSE] file.