![Tests failed](https://img.shields.io/badge/tests-268%20passed%2C%201%20failed-critical)
## ❌ <a id="user-content-r0" href="#r0">fixtures/external/flutter/provider-test-results.json</a>
**269** tests were completed in **0ms** with **268** passed, **1** failed and **0** skipped.
|Test suite|Passed|Failed|Skipped|Time|
|:---|---:|---:|---:|---:|
|[test/builder_test.dart](#r0s0)|24✔️|||402ms|
|[test/change_notifier_provider_test.dart](#r0s1)|10✔️|||306ms|
|[test/consumer_test.dart](#r0s2)|18✔️|||340ms|
|[test/context_test.dart](#r0s3)|31✔️|||698ms|
|[test/future_provider_test.dart](#r0s4)|10✔️|||305ms|
|[test/inherited_provider_test.dart](#r0s5)|81✔️|||1s|
|[test/listenable_provider_test.dart](#r0s6)|16✔️|||353ms|
|[test/listenable_proxy_provider_test.dart](#r0s7)|12✔️|||373ms|
|[test/multi_provider_test.dart](#r0s8)|3✔️|||198ms|
|[test/provider_test.dart](#r0s9)|11✔️|||306ms|
|[test/proxy_provider_test.dart](#r0s10)|16✔️|||438ms|
|[test/reassemble_test.dart](#r0s11)|3✔️|||221ms|
|[test/selector_test.dart](#r0s12)|17✔️|||364ms|
|[test/stateful_provider_test.dart](#r0s13)|4✔️|||254ms|
|[test/stream_provider_test.dart](#r0s14)|8✔️|||282ms|
|[test/value_listenable_provider_test.dart](#r0s15)|4✔️|1❌||327ms|
### ✔️ <a id="user-content-r0s0" href="#r0s0">test/builder_test.dart</a>
```
ChangeNotifierProvider
  ✔️ default
  ✔️ .value
ListenableProvider
  ✔️ default
  ✔️ .value
Provider
  ✔️ default
  ✔️ .value
ProxyProvider
  ✔️ 0
  ✔️ 1
  ✔️ 2
  ✔️ 3
  ✔️ 4
  ✔️ 5
  ✔️ 6
MultiProvider
  ✔️ with 1 ChangeNotifierProvider default
  ✔️ with 2 ChangeNotifierProvider default
  ✔️ with ListenableProvider default
  ✔️ with Provider default
  ✔️ with ProxyProvider0
  ✔️ with ProxyProvider1
  ✔️ with ProxyProvider2
  ✔️ with ProxyProvider3
  ✔️ with ProxyProvider4
  ✔️ with ProxyProvider5
  ✔️ with ProxyProvider6
```
### ✔️ <a id="user-content-r0s1" href="#r0s1">test/change_notifier_provider_test.dart</a>
```
✔️ Use builder property, not child
ChangeNotifierProvider
  ✔️ value
  ✔️ builder
  ✔️ builder1
  ✔️ builder2
  ✔️ builder3
  ✔️ builder4
  ✔️ builder5
  ✔️ builder6
  ✔️ builder0
```
### ✔️ <a id="user-content-r0s2" href="#r0s2">test/consumer_test.dart</a>
```
consumer
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
consumer2
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
consumer3
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
consumer4
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
consumer5
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
consumer6
  ✔️ obtains value from Provider<T>
  ✔️ crashed with no builder
  ✔️ can be used inside MultiProvider
```
### ✔️ <a id="user-content-r0s3" href="#r0s3">test/context_test.dart</a>
```
✔️ watch in layoutbuilder
✔️ select in layoutbuilder
✔️ cannot select in listView
✔️ watch in listView
✔️ watch in gridView
✔️ clears select dependencies for all dependents
BuildContext
  ✔️ internal selected value is updated
  ✔️ create can use read without being lazy
  ✔️ watch can be used inside InheritedProvider.update
  ✔️ select doesn't fail if it loads a provider that depends on other providers
  ✔️ don't call old selectors if the child rebuilds individually
  ✔️ selects throws inside click handlers
  ✔️ select throws if try to read dynamic
  ✔️ select throws ProviderNotFoundException
  ✔️ select throws if watch called inside the callback from build
  ✔️ select throws if read called inside the callback from build
  ✔️ select throws if select called inside the callback from build
  ✔️ select throws if read called inside the callback on dependency change
  ✔️ select throws if watch called inside the callback on dependency change
  ✔️ select throws if select called inside the callback on dependency change
  ✔️ can call read inside didChangeDependencies
  ✔️ select cannot be called inside didChangeDependencies
  ✔️ select in initState throws
  ✔️ watch in initState throws
  ✔️ read in initState works
  ✔️ consumer can be removed and selector stops to be called
  ✔️ context.select deeply compares maps
  ✔️ context.select deeply compares lists
  ✔️ context.select deeply compares iterables
  ✔️ context.select deeply compares sets
  ✔️ context.watch listens to value changes
```
### ✔️ <a id="user-content-r0s4" href="#r0s4">test/future_provider_test.dart</a>
```
✔️ works with MultiProvider
✔️ (catchError) previous future completes after transition is no-op
✔️ previous future completes after transition is no-op
✔️ transition from future to future preserve state
✔️ throws if future has error and catchError is missing
✔️ calls catchError if present and future has error
✔️ works with null
✔️ create and dispose future with builder
✔️ FutureProvider() crashes if builder is null
FutureProvider()
  ✔️ crashes if builder is null
```
### ✔️ <a id="user-content-r0s5" href="#r0s5">test/inherited_provider_test.dart</a>
```
✔️ regression test #377
✔️ rebuild on dependency flags update
✔️ properly update debug flags if a create triggers another deferred create
✔️ properly update debug flags if a create triggers another deferred create
✔️ properly update debug flags if an update triggers another create/update
✔️ properly update debug flags if a create triggers another create/update
✔️ Provider.of(listen: false) outside of build works when it loads a provider
✔️ new value is available in didChangeDependencies
✔️ builder receives the current value and updates independently from `update`
✔️ builder can _not_ rebuild when provider updates
✔️ builder rebuilds if provider is recreated
✔️ provider.of throws if listen:true outside of the widget tree
✔️ InheritedProvider throws if no child is provided with default constructor
✔️ InheritedProvider throws if no child is provided with value constructor
✔️ DeferredInheritedProvider throws if no child is provided with default constructor
✔️ DeferredInheritedProvider throws if no child is provided with value constructor
✔️ startListening markNeedsNotifyDependents
✔️ InheritedProvider can be subclassed
✔️ DeferredInheritedProvider can be subclassed
✔️ can be used with MultiProvider
✔️ throw if the widget ctor changes
✔️ InheritedProvider lazy loading can be disabled
✔️ InheritedProvider.value lazy loading can be disabled
✔️ InheritedProvider subclass don't have to specify default lazy value
✔️ DeferredInheritedProvider lazy loading can be disabled
✔️ DeferredInheritedProvider.value lazy loading can be disabled
✔️ selector
✔️ can select multiple types from same provider
✔️ can select same type on two different providers
✔️ can select same type twice on same provider
✔️ Provider.of has a proper error message if context is null
diagnostics
  ✔️ InheritedProvider.value
  ✔️ InheritedProvider doesn't break lazy loading
  ✔️ InheritedProvider show if listening
  ✔️ DeferredInheritedProvider.value
  ✔️ DeferredInheritedProvider
InheritedProvider.value()
  ✔️ markNeedsNotifyDependents during startListening is noop
  ✔️ startListening called again when create returns new value
  ✔️ startListening
  ✔️ stopListening not called twice if rebuild doesn't have listeners
  ✔️ removeListener cannot be null
  ✔️ pass down current value
  ✔️ default updateShouldNotify
  ✔️ custom updateShouldNotify
InheritedProvider()
  ✔️ hasValue
  ✔️ provider calls update if rebuilding only due to didChangeDependencies
  ✔️ provider notifying dependents doesn't call update
  ✔️ update can call Provider.of with listen:true
  ✔️ update lazy loaded can call Provider.of with listen:true
  ✔️ markNeedsNotifyDependents during startListening is noop
  ✔️ update can obtain parent of the same type than self
  ✔️ _debugCheckInvalidValueType
  ✔️ startListening
  ✔️ startListening called again when create returns new value
  ✔️ stopListening not called twice if rebuild doesn't have listeners
  ✔️ removeListener cannot be null
  ✔️ fails if initialValueBuilder calls inheritFromElement/inheritFromWiggetOfExactType
  ✔️ builder is called on every rebuild and after a dependency change
  ✔️ builder with no updateShouldNotify use ==
  ✔️ builder calls updateShouldNotify callback
  ✔️ initialValue is transmitted to valueBuilder
  ✔️ calls builder again if dependencies change
  ✔️ exposes initialValue if valueBuilder is null
  ✔️ call dispose on unmount
  ✔️ builder unmount, dispose not called if value never read
  ✔️ call dispose after new value
  ✔️ valueBuilder works without initialBuilder
  ✔️ calls initialValueBuilder lazily once
  ✔️ throws if both builder and initialBuilder are missing
DeferredInheritedProvider.value()
  ✔️ hasValue
  ✔️ startListening
  ✔️ stopListening cannot be null
  ✔️ startListening doesn't need setState if already initialized
  ✔️ setState without updateShouldNotify
  ✔️ setState with updateShouldNotify
  ✔️ startListening never leave the widget uninitialized
  ✔️ startListening called again on controller change
DeferredInheritedProvider()
  ✔️ create can't call inherited widgets
  ✔️ creates the value lazily
  ✔️ dispose
  ✔️ dispose no-op if never built
```
### ✔️ <a id="user-content-r0s6" href="#r0s6">test/listenable_provider_test.dart</a>
```
ListenableProvider
  ✔️ works with MultiProvider
  ✔️ asserts that the created notifier can have listeners
  ✔️ don't listen again if listenable instance doesn't change
  ✔️ works with null (default)
  ✔️ works with null (create)
  ✔️ stateful create called once
  ✔️ dispose called on unmount
  ✔️ dispose can be null
  ✔️ changing listenable rebuilds descendants
  ✔️ rebuilding with the same provider don't rebuilds descendants
  ✔️ notifylistener rebuilds descendants
ListenableProvider value constructor
  ✔️ pass down key
  ✔️ changing the Listenable instance rebuilds dependents
ListenableProvider stateful constructor
  ✔️ called with context
  ✔️ pass down key
  ✔️ throws if create is null
```
### ✔️ <a id="user-content-r0s7" href="#r0s7">test/listenable_proxy_provider_test.dart</a>
```
ListenableProxyProvider
  ✔️ throws if update is missing
  ✔️ asserts that the created notifier has no listener
  ✔️ asserts that the created notifier has no listener after rebuild
  ✔️ rebuilds dependendents when listeners are called
  ✔️ update returning a new Listenable disposes the previously created value and update dependents
  ✔️ disposes of created value
ListenableProxyProvider variants
  ✔️ ListenableProxyProvider
  ✔️ ListenableProxyProvider2
  ✔️ ListenableProxyProvider3
  ✔️ ListenableProxyProvider4
  ✔️ ListenableProxyProvider5
  ✔️ ListenableProxyProvider6
```
### ✔️ <a id="user-content-r0s8" href="#r0s8">test/multi_provider_test.dart</a>
```
MultiProvider
  ✔️ throw if providers is null
  ✔️ MultiProvider children can only access parent providers
  ✔️ MultiProvider.providers with ignored child
```
### ✔️ <a id="user-content-r0s9" href="#r0s9">test/provider_test.dart</a>
```
✔️ works with MultiProvider
Provider.of
  ✔️ throws if T is dynamic
  ✔️ listen defaults to true when building widgets
  ✔️ listen defaults to false outside of the widget tree
  ✔️ listen:false doesn't trigger rebuild
  ✔️ listen:true outside of the widget tree throws
Provider
  ✔️ throws if the provided value is a Listenable/Stream
  ✔️ debugCheckInvalidValueType can be disabled
  ✔️ simple usage
  ✔️ throws an error if no provider found
  ✔️ update should notify
```
### ✔️ <a id="user-content-r0s10" href="#r0s10">test/proxy_provider_test.dart</a>
```
ProxyProvider
  ✔️ throws if the provided value is a Listenable/Stream
  ✔️ debugCheckInvalidValueType can be disabled
  ✔️ create creates initial value
  ✔️ consume another providers
  ✔️ rebuild descendants if value change
  ✔️ call dispose when unmounted with the latest result
  ✔️ don't rebuild descendants if value doesn't change
  ✔️ pass down updateShouldNotify
  ✔️ works with MultiProvider
  ✔️ update callback can trigger descendants setState synchronously
  ✔️ throws if update is null
ProxyProvider variants
  ✔️ ProxyProvider2
  ✔️ ProxyProvider3
  ✔️ ProxyProvider4
  ✔️ ProxyProvider5
  ✔️ ProxyProvider6
```
### ✔️ <a id="user-content-r0s11" href="#r0s11">test/reassemble_test.dart</a>
```
✔️ ReassembleHandler
✔️ unevaluated create
✔️ unevaluated create
```
### ✔️ <a id="user-content-r0s12" href="#r0s12">test/selector_test.dart</a>
```
✔️ asserts that builder/selector are not null
✔️ Deep compare maps by default
✔️ Deep compare iterables by default
✔️ Deep compare sets by default
✔️ Deep compare lists by default
✔️ custom shouldRebuid
✔️ passes `child` and `key`
✔️ calls builder if the callback changes
✔️ works with MultiProvider
✔️ don't call builder again if it rebuilds but selector returns the same thing
✔️ call builder again if it rebuilds abd selector returns the a different variable
✔️ Selector
✔️ Selector2
✔️ Selector3
✔️ Selector4
✔️ Selector5
✔️ Selector6
```
### ✔️ <a id="user-content-r0s13" href="#r0s13">test/stateful_provider_test.dart</a>
```
✔️ asserts
✔️ works with MultiProvider
✔️ calls create only once
✔️ dispose
```
### ✔️ <a id="user-content-r0s14" href="#r0s14">test/stream_provider_test.dart</a>
```
✔️ works with MultiProvider
✔️ transition from stream to stream preserve state
✔️ throws if stream has error and catchError is missing
✔️ calls catchError if present and stream has error
✔️ works with null
✔️ StreamProvider() crashes if builder is null
StreamProvider()
  ✔️ create and dispose stream with builder
  ✔️ crashes if builder is null
```
### ❌ <a id="user-content-r0s15" href="#r0s15">test/value_listenable_provider_test.dart</a>
```
valueListenableProvider
  ✔️ rebuilds when value change
  ✔️ don't rebuild dependents by default
  ✔️ pass keys
  ✔️ don't listen again if stream instance doesn't change
  ❌ pass updateShouldNotify
	The following TestFailure object was thrown running a test:
	  Expected: <2>
	  Actual: <1>
	Unexpected number of calls
	
```