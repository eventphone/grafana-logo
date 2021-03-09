[![Build status](https://ci.appveyor.com/api/projects/status/gel79fj1ivr569l7/branch/master?svg=true)](https://ci.appveyor.com/project/eventphone/grafana-logo/branch/master)

## What?

This is a small webservice implementing a graphite compatible endpoint to render logos via metrics on your dashboards. This project was inspired by [Monitoring Art](https://github.com/monitoringartist/grafana-monitoring-art).

## Why?

Because it's possible.

## Can I take a look?

![simplejson-logo](doc/sj-logo.png)
Also checkout the dashboards from [#eh18](https://youtu.be/5eguMOTkq_8).

## How

### run from release

- download [latest release](https://github.com/eventphone/grafana-logo/releases/latest)
- install [aspnetcore runtime](https://www.microsoft.com/net/download)
- unzip
- `dotnet grafana-logo.dll`

### or run from source

``` sh
$ git clone https://github.com/eventphone/grafana-logo.git
$ cd src/grafana-logo
$ dotnet run
```

### add a datasource to grafana

- Name: choose a meaningful name
- Type: Graphite
- Url: http://localhost:5000
- Access: proxy

![grafana datasource](doc/datasource.png)

### upload your image

```sh
cd src/grafana-logo/wwwroot/images
wget https://github.com/eventphone/grafana-logo/raw/master/src/wwwroot/images/eventphone_logo_schriftzug.png
```

### create a new graph

- Datasource: your meaningful name
- Metric: timeseries - filename of your logo

![Metrics](doc/metrics.png)

- switch off legends: Legend -> Show
- enable stack: Display -> Stack
- remove lines: Display -> Line Width -> 0)
- add color: Display -> Fill -> 10

![Display](doc/display.png)

- add overrides:
  - Display -> Series override
  - add an override for each color
    - alias or regex: select color code
    - \+ color: set the color code from the name
  - set Lines=false for the background color

![Overrides](doc/overrides.png)