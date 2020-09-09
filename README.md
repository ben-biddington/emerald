# Requirements

## .NET Core 3.1 SDK

### Ubuntu 

https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu

```shell
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && sudo dpkg -i packages-microsoft-prod.deb
```

```shell
sudo apt-get update && sudo apt-get install -y apt-transport-https && sudo apt-get update && sudo apt-get install -y dotnet-sdk-3.1
```

# Usage

```shell
$ ./run --build https://www.rnz.co.nz/ --dir ./screenshots
```

Or run in container:

```shell
$ ./run --container https://www.rnz.co.nz/
```

Force container (re)build:

```shell
$ ./run --container --build https://www.rnz.co.nz/
```

<p align="center">
  <img src="https://github.com/ben-biddington/emerald/raw/trunk/doc/https---www.rnz.co.nz--637352439552501962.png">
</p>