# Dapplo.InterfaceImpl

- Documentation can be found [here](http://www.dapplo.net/blocks/Dapplo.InterfaceImpl) (soon)
- Current build status: [![Build status](https://ci.appveyor.com/api/projects/status/rskeg5hqeu9k2crs?svg=true)](https://ci.appveyor.com/project/dapplo/dapplo-interfaceimpl)
- Coverage Status: [![Coverage Status](https://coveralls.io/repos/github/dapplo/Dapplo.InterfaceImpl/badge.svg?branch=master)](https://coveralls.io/github/dapplo/Dapplo.InterfaceImpl?branch=master)

## Additional functionality

As the framework generates an implementation of your interface, it can extended this with additional functionality.
You can use this simply by extending your interface with additional interfaces.

Currently the following is available but can be extended:
* INotifyPropertyChanged: this will add the PropertyChanged event, and generate events for every property that you changed.
* INotifyPropertyChanging: this will add the PropertyChanging event, and generate events for every property that you are changing.
* IHasChanges: this will add a possibility to detect if you changed a value since the start or a reset.
* IDescription: this will add method to get the value of a DescriptionAttribute of a property.
* ITransactionalProperties: this will add rollback and commit to your implementation
