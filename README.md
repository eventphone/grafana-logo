## What?
This is a small webservice implementing a Grafana SimpleJson Endpoint to render logos on your dashboards. This project was inspired by [Monitoring Art](https://github.com/monitoringartist/grafana-monitoring-art).

## Why?
Because it's possible.

## Can I take a look?
Checkout the dashboards from [#eh18](https://youtu.be/5eguMOTkq_8).

## How

#### checkout & run
``` sh
$ git clone https://github.com/eventphone/grafana-logo.git
$ cd src/grafana-logo
$ dotnet run
```
#### install [SimpleJson Plugin](https://grafana.com/plugins/grafana-simple-json-datasource/installation)
``` sh
grafana-cli plugins install grafana-simple-json-datasource
```
#### add a datasource to grafana
* Name: choose a meaningful name
* Type: SimpleJson
* Url: http://localhost:5000
* Access: proxy

![grafana datasource](https://github.com/eventphone/grafana-logo/raw/master/doc/datasource.png)

#### upload your logo image
```sh
cd src/grafana-logo/wwwroot/images
wget https://github.com/eventphone/grafana-logo/raw/master/src/wwwroot/images/eventphone_logo_schriftzug.png
```

#### create a new graph
* Datasource: your meaningful name
* Metric: timeseries - filename of your logo

![Metrics](https://github.com/eventphone/grafana-logo/raw/master/doc/metrics.png)

* switch off legends: Legend -> Show
* enable stack: Display -> Stack
* remove lines: Display -> Line Width -> 0)
* add color: Display -> Fill -> 10

![Display](https://github.com/eventphone/grafana-logo/raw/master/doc/display.png)

* add overrides:
  * Display -> Series override
  * add an override for each color
    * alias or regex: select color code
    * \+ color: set the color code from the name
  * set Lines=false for the background color

![Overrides](https://github.com/eventphone/grafana-logo/raw/master/doc/overrides.png)