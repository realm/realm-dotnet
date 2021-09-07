![Tests failed](https://img.shields.io/badge/tests-793%20passed%2C%201%20failed%2C%2014%20skipped-critical)
## ❌ <a id="user-content-r0" href="#r0">fixtures/external/java/pulsar-test-report.xml</a>
**808** tests were completed in **2127s** with **793** passed, **1** failed and **14** skipped.
|Test suite|Passed|Failed|Skipped|Time|
|:---|---:|---:|---:|---:|
|[org.apache.pulsar.AddMissingPatchVersionTest](#r0s0)||1❌|1✖️|116ms|
|[org.apache.pulsar.broker.admin.AdminApiOffloadTest](#r0s1)|7✔️|||19s|
|[org.apache.pulsar.broker.auth.AuthenticationServiceTest](#r0s2)|2✔️|||185ms|
|[org.apache.pulsar.broker.auth.AuthLogsTest](#r0s3)|2✔️|||1s|
|[org.apache.pulsar.broker.auth.AuthorizationTest](#r0s4)|1✔️|||2s|
|[org.apache.pulsar.broker.lookup.http.HttpTopicLookupv2Test](#r0s5)|4✔️|||2s|
|[org.apache.pulsar.broker.namespace.NamespaceCreateBundlesTest](#r0s6)|2✔️|||33s|
|[org.apache.pulsar.broker.namespace.NamespaceOwnershipListenerTests](#r0s7)|2✔️|||32s|
|[org.apache.pulsar.broker.namespace.NamespaceServiceTest](#r0s8)|10✔️|||75s|
|[org.apache.pulsar.broker.namespace.NamespaceUnloadingTest](#r0s9)|2✔️|||14s|
|[org.apache.pulsar.broker.namespace.OwnerShipCacheForCurrentServerTest](#r0s10)|1✔️|||16s|
|[org.apache.pulsar.broker.namespace.OwnershipCacheTest](#r0s11)|8✔️|||16s|
|[org.apache.pulsar.broker.protocol.ProtocolHandlersTest](#r0s12)|6✔️|||946ms|
|[org.apache.pulsar.broker.protocol.ProtocolHandlerUtilsTest](#r0s13)|3✔️|||7s|
|[org.apache.pulsar.broker.protocol.ProtocolHandlerWithClassLoaderTest](#r0s14)|1✔️|||15ms|
|[org.apache.pulsar.broker.PulsarServiceTest](#r0s15)|2✔️|||96ms|
|[org.apache.pulsar.broker.service.MessagePublishBufferThrottleTest](#r0s16)|3✔️|||14s|
|[org.apache.pulsar.broker.service.ReplicatorTest](#r0s17)|22✔️|||40s|
|[org.apache.pulsar.broker.service.TopicOwnerTest](#r0s18)|8✔️|||114s|
|[org.apache.pulsar.broker.SLAMonitoringTest](#r0s19)|4✔️|||9s|
|[org.apache.pulsar.broker.stats.BookieClientsStatsGeneratorTest](#r0s20)|2✔️|||49ms|
|[org.apache.pulsar.broker.stats.ConsumerStatsTest](#r0s21)|3✔️|||21s|
|[org.apache.pulsar.broker.stats.ManagedCursorMetricsTest](#r0s22)|1✔️|||281ms|
|[org.apache.pulsar.broker.stats.ManagedLedgerMetricsTest](#r0s23)|1✔️|||285ms|
|[org.apache.pulsar.broker.stats.prometheus.AggregatedNamespaceStatsTest](#r0s24)|1✔️|||40ms|
|[org.apache.pulsar.broker.stats.PrometheusMetricsTest](#r0s25)|15✔️|||83s|
|[org.apache.pulsar.broker.stats.SubscriptionStatsTest](#r0s26)|2✔️|||2s|
|[org.apache.pulsar.broker.systopic.NamespaceEventsSystemTopicServiceTest](#r0s27)|1✔️|||1s|
|[org.apache.pulsar.broker.transaction.buffer.InMemTransactionBufferReaderTest](#r0s28)|3✔️|||28ms|
|[org.apache.pulsar.broker.transaction.buffer.TransactionBufferClientTest](#r0s29)|4✔️|||93ms|
|[org.apache.pulsar.broker.transaction.buffer.TransactionBufferTest](#r0s30)|7✔️|||81ms|
|[org.apache.pulsar.broker.transaction.buffer.TransactionEntryImplTest](#r0s31)|1✔️|||14ms|
|[org.apache.pulsar.broker.transaction.buffer.TransactionLowWaterMarkTest](#r0s32)|2✔️|||38s|
|[org.apache.pulsar.broker.transaction.buffer.TransactionStablePositionTest](#r0s33)|2✔️||1✖️|49s|
|[org.apache.pulsar.broker.transaction.coordinator.TransactionCoordinatorClientTest](#r0s34)|3✔️|||95ms|
|[org.apache.pulsar.broker.transaction.coordinator.TransactionMetaStoreAssignmentTest](#r0s35)|1✔️|||1s|
|[org.apache.pulsar.broker.transaction.pendingack.PendingAckInMemoryDeleteTest](#r0s36)|2✔️||1✖️|57s|
|[org.apache.pulsar.broker.transaction.TransactionConsumeTest](#r0s37)|2✔️|||30s|
|[org.apache.pulsar.broker.web.RestExceptionTest](#r0s38)|3✔️|||37ms|
|[org.apache.pulsar.broker.web.WebServiceTest](#r0s39)|9✔️|||27s|
|[org.apache.pulsar.client.impl.AdminApiKeyStoreTlsAuthTest](#r0s40)|4✔️|||8s|
|[org.apache.pulsar.client.impl.BatchMessageIdImplSerializationTest](#r0s41)|4✔️|||30ms|
|[org.apache.pulsar.client.impl.BatchMessageIndexAckDisableTest](#r0s42)|4✔️|||14s|
|[org.apache.pulsar.client.impl.BatchMessageIndexAckTest](#r0s43)|5✔️|||44s|
|[org.apache.pulsar.client.impl.BrokerClientIntegrationTest](#r0s44)|15✔️|||148s|
|[org.apache.pulsar.client.impl.CompactedOutBatchMessageTest](#r0s45)|1✔️|||1s|
|[org.apache.pulsar.client.impl.ConsumerAckResponseTest](#r0s46)|1✔️|||549ms|
|[org.apache.pulsar.client.impl.ConsumerConfigurationTest](#r0s47)|4✔️|||12s|
|[org.apache.pulsar.client.impl.ConsumerDedupPermitsUpdate](#r0s48)|7✔️|||4s|
|[org.apache.pulsar.client.impl.ConsumerUnsubscribeTest](#r0s49)|1✔️|||129ms|
|[org.apache.pulsar.client.impl.KeyStoreTlsProducerConsumerTestWithAuth](#r0s50)|3✔️|||23s|
|[org.apache.pulsar.client.impl.KeyStoreTlsProducerConsumerTestWithoutAuth](#r0s51)|3✔️|||8s|
|[org.apache.pulsar.client.impl.KeyStoreTlsTest](#r0s52)|1✔️|||183ms|
|[org.apache.pulsar.client.impl.MessageChecksumTest](#r0s53)|3✔️|||47s|
|[org.apache.pulsar.client.impl.MessageChunkingTest](#r0s54)|8✔️||1✖️|73s|
|[org.apache.pulsar.client.impl.MessageParserTest](#r0s55)|2✔️|||5s|
|[org.apache.pulsar.client.impl.MultiTopicsReaderTest](#r0s56)|8✔️|||35s|
|[org.apache.pulsar.client.impl.NegativeAcksTest](#r0s57)|32✔️|||11s|
|[org.apache.pulsar.client.impl.PatternTopicsConsumerImplTest](#r0s58)|11✔️|||63s|
|[org.apache.pulsar.client.impl.PerMessageUnAcknowledgedRedeliveryTest](#r0s59)|5✔️|||34s|
|[org.apache.pulsar.client.impl.PulsarMultiHostClientTest](#r0s60)|3✔️|||15s|
|[org.apache.pulsar.client.impl.RawMessageSerDeserTest](#r0s61)|1✔️|||10ms|
|[org.apache.pulsar.client.impl.SchemaDeleteTest](#r0s62)|1✔️|||2s|
|[org.apache.pulsar.client.impl.SequenceIdWithErrorTest](#r0s63)|3✔️||2✖️|18s|
|[org.apache.pulsar.client.impl.TopicDoesNotExistsTest](#r0s64)|2✔️|||4s|
|[org.apache.pulsar.client.impl.TopicFromMessageTest](#r0s65)|5✔️|||14s|
|[org.apache.pulsar.client.impl.TopicsConsumerImplTest](#r0s66)|17✔️|||133s|
|[org.apache.pulsar.client.impl.UnAcknowledgedMessagesTimeoutTest](#r0s67)|7✔️|||44s|
|[org.apache.pulsar.client.impl.ZeroQueueSizeTest](#r0s68)|14✔️|||16s|
|[org.apache.pulsar.common.api.raw.RawMessageImplTest](#r0s69)|1✔️|||316ms|
|[org.apache.pulsar.common.compression.CommandsTest](#r0s70)|1✔️|||30ms|
|[org.apache.pulsar.common.compression.CompressorCodecBackwardCompatTest](#r0s71)|6✔️|||223ms|
|[org.apache.pulsar.common.compression.CompressorCodecTest](#r0s72)|45✔️|||737ms|
|[org.apache.pulsar.common.compression.Crc32cChecksumTest](#r0s73)|6✔️|||5s|
|[org.apache.pulsar.common.lookup.data.LookupDataTest](#r0s74)|4✔️|||2s|
|[org.apache.pulsar.common.naming.MetadataTests](#r0s75)|2✔️|||161ms|
|[org.apache.pulsar.common.naming.NamespaceBundlesTest](#r0s76)|5✔️|||99ms|
|[org.apache.pulsar.common.naming.NamespaceBundleTest](#r0s77)|6✔️|||64ms|
|[org.apache.pulsar.common.naming.NamespaceNameTest](#r0s78)|2✔️|||207ms|
|[org.apache.pulsar.common.naming.ServiceConfigurationTest](#r0s79)|4✔️|||48ms|
|[org.apache.pulsar.common.naming.TopicNameTest](#r0s80)|4✔️|||529ms|
|[org.apache.pulsar.common.net.ServiceURITest](#r0s81)|21✔️|||237ms|
|[org.apache.pulsar.common.policies.data.AutoFailoverPolicyDataTest](#r0s82)|1✔️|||15ms|
|[org.apache.pulsar.common.policies.data.AutoFailoverPolicyTypeTest](#r0s83)|1✔️|||19ms|
|[org.apache.pulsar.common.policies.data.AutoTopicCreationOverrideTest](#r0s84)|6✔️|||64ms|
|[org.apache.pulsar.common.policies.data.BacklogQuotaTest](#r0s85)|1✔️|||12ms|
|[org.apache.pulsar.common.policies.data.ClusterDataTest](#r0s86)|1✔️|||9ms|
|[org.apache.pulsar.common.policies.data.ConsumerStatsTest](#r0s87)|1✔️|||8ms|
|[org.apache.pulsar.common.policies.data.EnsemblePlacementPolicyConfigTest](#r0s88)|2✔️|||948ms|
|[org.apache.pulsar.common.policies.data.LocalPolicesTest](#r0s89)|1✔️|||48ms|
|[org.apache.pulsar.common.policies.data.NamespaceIsolationDataTest](#r0s90)|1✔️|||76ms|
|[org.apache.pulsar.common.policies.data.NamespaceOwnershipStatusTest](#r0s91)|1✔️|||45ms|
|[org.apache.pulsar.common.policies.data.OffloadPoliciesTest](#r0s92)|6✔️|||216ms|
|[org.apache.pulsar.common.policies.data.PartitionedTopicStatsTest](#r0s93)|1✔️|||12ms|
|[org.apache.pulsar.common.policies.data.PersistencePoliciesTest](#r0s94)|1✔️|||19ms|
|[org.apache.pulsar.common.policies.data.PersistentOfflineTopicStatsTest](#r0s95)|1✔️|||29ms|
|[org.apache.pulsar.common.policies.data.PersistentTopicStatsTest](#r0s96)|2✔️|||51ms|
|[org.apache.pulsar.common.policies.data.PoliciesDataTest](#r0s97)|4✔️|||1s|
|[org.apache.pulsar.common.policies.data.PublisherStatsTest](#r0s98)|2✔️|||37ms|
|[org.apache.pulsar.common.policies.data.ReplicatorStatsTest](#r0s99)|2✔️|||30ms|
|[org.apache.pulsar.common.policies.data.ResourceQuotaTest](#r0s100)|2✔️|||45ms|
|[org.apache.pulsar.common.policies.data.RetentionPolicesTest](#r0s101)|1✔️|||8ms|
|[org.apache.pulsar.common.policies.impl.AutoFailoverPolicyFactoryTest](#r0s102)|1✔️|||22ms|
|[org.apache.pulsar.common.policies.impl.MinAvailablePolicyTest](#r0s103)|1✔️|||1ms|
|[org.apache.pulsar.common.policies.impl.NamespaceIsolationPoliciesTest](#r0s104)|7✔️|||265ms|
|[org.apache.pulsar.common.policies.impl.NamespaceIsolationPolicyImplTest](#r0s105)|7✔️|||309ms|
|[org.apache.pulsar.common.protocol.ByteBufPairTest](#r0s106)|2✔️|||5s|
|[org.apache.pulsar.common.protocol.CommandUtilsTests](#r0s107)|7✔️|||3s|
|[org.apache.pulsar.common.protocol.MarkersTest](#r0s108)|6✔️|||3s|
|[org.apache.pulsar.common.protocol.PulsarDecoderTest](#r0s109)|1✔️|||4s|
|[org.apache.pulsar.common.stats.JvmDefaultGCMetricsLoggerTest](#r0s110)|1✔️|||82ms|
|[org.apache.pulsar.common.util.collections.BitSetRecyclableRecyclableTest](#r0s111)|2✔️|||13ms|
|[org.apache.pulsar.common.util.collections.ConcurrentBitSetRecyclableTest](#r0s112)|2✔️|||63ms|
|[org.apache.pulsar.common.util.collections.ConcurrentLongHashMapTest](#r0s113)|13✔️|||28s|
|[org.apache.pulsar.common.util.collections.ConcurrentLongPairSetTest](#r0s114)|15✔️|||2s|
|[org.apache.pulsar.common.util.collections.ConcurrentOpenHashMapTest](#r0s115)|12✔️|||9s|
|[org.apache.pulsar.common.util.collections.ConcurrentOpenHashSetTest](#r0s116)|11✔️|||7s|
|[org.apache.pulsar.common.util.collections.ConcurrentOpenLongPairRangeSetTest](#r0s117)|13✔️|||1s|
|[org.apache.pulsar.common.util.collections.ConcurrentSortedLongPairSetTest](#r0s118)|9✔️|||342ms|
|[org.apache.pulsar.common.util.collections.FieldParserTest](#r0s119)|2✔️|||64ms|
|[org.apache.pulsar.common.util.collections.GrowableArrayBlockingQueueTest](#r0s120)|6✔️|||350ms|
|[org.apache.pulsar.common.util.collections.GrowablePriorityLongPairQueueTest](#r0s121)|15✔️|||3s|
|[org.apache.pulsar.common.util.collections.TripleLongPriorityQueueTest](#r0s122)|3✔️|||238ms|
|[org.apache.pulsar.common.util.FieldParserTest](#r0s123)|1✔️|||242ms|
|[org.apache.pulsar.common.util.FileModifiedTimeUpdaterTest](#r0s124)|6✔️|||6s|
|[org.apache.pulsar.common.util.netty.ChannelFuturesTest](#r0s125)|5✔️|||2s|
|[org.apache.pulsar.common.util.RateLimiterTest](#r0s126)|11✔️|||7s|
|[org.apache.pulsar.common.util.ReflectionsTest](#r0s127)|12✔️|||172ms|
|[org.apache.pulsar.common.util.RelativeTimeUtilTest](#r0s128)|1✔️|||39ms|
|[org.apache.pulsar.discovery.service.web.DiscoveryServiceWebTest](#r0s129)|1✔️|||5s|
|[org.apache.pulsar.functions.worker.PulsarFunctionE2ESecurityTest](#r0s130)|2✔️|||28s|
|[org.apache.pulsar.functions.worker.PulsarFunctionPublishTest](#r0s131)|3✔️|||42s|
|[org.apache.pulsar.functions.worker.PulsarFunctionTlsTest](#r0s132)|1✔️|||12s|
|[org.apache.pulsar.io.PulsarFunctionTlsTest](#r0s133)|1✔️|||30s|
|[org.apache.pulsar.proxy.server.AdminProxyHandlerTest](#r0s134)|1✔️|||474ms|
|[org.apache.pulsar.proxy.server.AuthedAdminProxyHandlerTest](#r0s135)|1✔️|||2s|
|[org.apache.pulsar.proxy.server.FunctionWorkerRoutingTest](#r0s136)|1✔️|||10ms|
|[org.apache.pulsar.proxy.server.ProxyAdditionalServletTest](#r0s137)|1✔️|||125ms|
|[org.apache.pulsar.proxy.server.ProxyAuthenticatedProducerConsumerTest](#r0s138)|1✔️|||2s|
|[org.apache.pulsar.proxy.server.ProxyAuthenticationTest](#r0s139)|1✔️|||17s|
|[org.apache.pulsar.proxy.server.ProxyConnectionThrottlingTest](#r0s140)|1✔️|||2s|
|[org.apache.pulsar.proxy.server.ProxyEnableHAProxyProtocolTest](#r0s141)|1✔️|||511ms|
|[org.apache.pulsar.proxy.server.ProxyForwardAuthDataTest](#r0s142)|1✔️|||32s|
|[org.apache.pulsar.proxy.server.ProxyIsAHttpProxyTest](#r0s143)|10✔️|||2s|
|[org.apache.pulsar.proxy.server.ProxyKeyStoreTlsTestWithAuth](#r0s144)|3✔️|||7s|
|[org.apache.pulsar.proxy.server.ProxyKeyStoreTlsTestWithoutAuth](#r0s145)|3✔️|||7s|
|[org.apache.pulsar.proxy.server.ProxyLookupThrottlingTest](#r0s146)|1✔️|||3s|
|[org.apache.pulsar.proxy.server.ProxyParserTest](#r0s147)|5✔️|||1s|
|[org.apache.pulsar.proxy.server.ProxyRolesEnforcementTest](#r0s148)|1✔️|||10s|
|[org.apache.pulsar.proxy.server.ProxyStatsTest](#r0s149)|3✔️|||533ms|
|[org.apache.pulsar.proxy.server.ProxyTest](#r0s150)|6✔️|||3s|
|[org.apache.pulsar.proxy.server.ProxyTlsTest](#r0s151)|2✔️|||414ms|
|[org.apache.pulsar.proxy.server.ProxyTlsTestWithAuth](#r0s152)|1✔️|||4ms|
|[org.apache.pulsar.proxy.server.ProxyWithAuthorizationNegTest](#r0s153)|1✔️|||2s|
|[org.apache.pulsar.proxy.server.ProxyWithAuthorizationTest](#r0s154)|13✔️|||33s|
|[org.apache.pulsar.proxy.server.ProxyWithoutServiceDiscoveryTest](#r0s155)|1✔️|||2s|
|[org.apache.pulsar.proxy.server.SuperUserAuthedAdminProxyHandlerTest](#r0s156)|3✔️|||8s|
|[org.apache.pulsar.proxy.server.UnauthedAdminProxyHandlerTest](#r0s157)|2✔️|||114ms|
|[org.apache.pulsar.PulsarBrokerStarterTest](#r0s158)|9✔️|||591ms|
|[org.apache.pulsar.schema.compatibility.SchemaCompatibilityCheckTest](#r0s159)|23✔️|||107s|
|[org.apache.pulsar.schema.PartitionedTopicSchemaTest](#r0s160)|1✔️|||29s|
|[org.apache.pulsar.schema.SchemaTest](#r0s161)|3✔️|||31s|
|[org.apache.pulsar.stats.client.PulsarBrokerStatsClientTest](#r0s162)|2✔️|||41s|
|[org.apache.pulsar.tests.EnumValuesDataProviderTest](#r0s163)|6✔️|||23ms|
|[org.apache.pulsar.tests.TestRetrySupportBeforeMethodRetryTest](#r0s164)|1✔️||4✖️|36ms|
|[org.apache.pulsar.tests.TestRetrySupportRetryTest](#r0s165)|1✔️||4✖️|27ms|
|[org.apache.pulsar.tests.TestRetrySupportSuccessTest](#r0s166)|3✔️|||1ms|
|[org.apache.pulsar.tests.ThreadDumpUtilTest](#r0s167)|2✔️|||17ms|
|[org.apache.pulsar.utils.SimpleTextOutputStreamTest](#r0s168)|4✔️|||50ms|
|[org.apache.pulsar.utils.StatsOutputStreamTest](#r0s169)|6✔️|||59ms|
|[org.apache.pulsar.websocket.proxy.ProxyAuthenticationTest](#r0s170)|4✔️|||29s|
|[org.apache.pulsar.websocket.proxy.ProxyAuthorizationTest](#r0s171)|1✔️|||1s|
|[org.apache.pulsar.websocket.proxy.ProxyConfigurationTest](#r0s172)|2✔️|||9s|
|[org.apache.pulsar.websocket.proxy.ProxyPublishConsumeTlsTest](#r0s173)|1✔️|||11s|
|[org.apache.pulsar.websocket.proxy.ProxyPublishConsumeWithoutZKTest](#r0s174)|1✔️|||7s|
|[org.apache.pulsar.websocket.proxy.v1.V1_ProxyAuthenticationTest](#r0s175)|4✔️|||30s|
### ❌ <a id="user-content-r0s0" href="#r0s0">org.apache.pulsar.AddMissingPatchVersionTest</a>
```
✖️ testVersionStrings
❌ testVersionStrings
	java.lang.AssertionError: expected [1.2.1] but found [1.2.0]
```
### ✔️ <a id="user-content-r0s1" href="#r0s1">org.apache.pulsar.broker.admin.AdminApiOffloadTest</a>
```
✔️ testOffloadPoliciesAppliedApi
✔️ testOffloadV2
✔️ testTopicLevelOffloadNonPartitioned
✔️ testTopicLevelOffloadPartitioned
✔️ testOffloadV1
✔️ testOffloadPolicies
✔️ testOffloadPoliciesApi
```
### ✔️ <a id="user-content-r0s2" href="#r0s2">org.apache.pulsar.broker.auth.AuthenticationServiceTest</a>
```
✔️ testAuthentication
✔️ testAuthenticationHttp
```
### ✔️ <a id="user-content-r0s3" href="#r0s3">org.apache.pulsar.broker.auth.AuthLogsTest</a>
```
✔️ httpEndpoint
✔️ binaryEndpoint
```
### ✔️ <a id="user-content-r0s4" href="#r0s4">org.apache.pulsar.broker.auth.AuthorizationTest</a>
```
✔️ simple
```
### ✔️ <a id="user-content-r0s5" href="#r0s5">org.apache.pulsar.broker.lookup.http.HttpTopicLookupv2Test</a>
```
✔️ crossColoLookup
✔️ testNotEnoughLookupPermits
✔️ testValidateReplicationSettingsOnNamespace
✔️ testDataPojo
```
### ✔️ <a id="user-content-r0s6" href="#r0s6">org.apache.pulsar.broker.namespace.NamespaceCreateBundlesTest</a>
```
✔️ testCreateNamespaceWithDefaultBundles
✔️ testSplitBundleUpdatesLocalPoliciesWithoutOverwriting
```
### ✔️ <a id="user-content-r0s7" href="#r0s7">org.apache.pulsar.broker.namespace.NamespaceOwnershipListenerTests</a>
```
✔️ testGetAllPartitions
✔️ testNamespaceBundleOwnershipListener
```
### ✔️ <a id="user-content-r0s8" href="#r0s8">org.apache.pulsar.broker.namespace.NamespaceServiceTest</a>
```
✔️ testSplitMapWithRefreshedStatMap
✔️ testRemoveOwnershipNamespaceBundle
✔️ testIsServiceUnitDisabled
✔️ testLoadReportDeserialize
✔️ testCreateLookupResult
✔️ testUnloadNamespaceBundleWithStuckTopic
✔️ testUnloadNamespaceBundleFailure
✔️ testSplitAndOwnBundles
✔️ testCreateNamespaceWithDefaultNumberOfBundles
✔️ testRemoveOwnershipAndSplitBundle
```
### ✔️ <a id="user-content-r0s9" href="#r0s9">org.apache.pulsar.broker.namespace.NamespaceUnloadingTest</a>
```
✔️ testUnloadNotLoadedNamespace
✔️ testUnloadPartiallyLoadedNamespace
```
### ✔️ <a id="user-content-r0s10" href="#r0s10">org.apache.pulsar.broker.namespace.OwnerShipCacheForCurrentServerTest</a>
```
✔️ testOwnershipForCurrentServer
```
### ✔️ <a id="user-content-r0s11" href="#r0s11">org.apache.pulsar.broker.namespace.OwnershipCacheTest</a>
```
✔️ testGetOwnedServiceUnits
✔️ testRemoveOwnership
✔️ testGetOwnedServiceUnit
✔️ testGetOrSetOwner
✔️ testConstructor
✔️ testGetOwner
✔️ testDisableOwnership
✔️ testReestablishOwnership
```
### ✔️ <a id="user-content-r0s12" href="#r0s12">org.apache.pulsar.broker.protocol.ProtocolHandlersTest</a>
```
✔️ testStart
✔️ testGetProtocol
✔️ testNewChannelInitializersSuccess
✔️ testInitialize
✔️ testNewChannelInitializersOverlapped
✔️ testGetProtocolDataToAdvertise
```
### ✔️ <a id="user-content-r0s13" href="#r0s13">org.apache.pulsar.broker.protocol.ProtocolHandlerUtilsTest</a>
```
✔️ testLoadProtocolHandler
✔️ testLoadProtocolHandlerBlankHandlerClass
✔️ testLoadProtocolHandlerWrongHandlerClass
```
### ✔️ <a id="user-content-r0s14" href="#r0s14">org.apache.pulsar.broker.protocol.ProtocolHandlerWithClassLoaderTest</a>
```
✔️ testWrapper
```
### ✔️ <a id="user-content-r0s15" href="#r0s15">org.apache.pulsar.broker.PulsarServiceTest</a>
```
✔️ testGetWorkerService
✔️ testGetWorkerServiceException
```
### ✔️ <a id="user-content-r0s16" href="#r0s16">org.apache.pulsar.broker.service.MessagePublishBufferThrottleTest</a>
```
✔️ testMessagePublishBufferThrottleEnable
✔️ testBlockByPublishRateLimiting
✔️ testMessagePublishBufferThrottleDisabled
```
### ✔️ <a id="user-content-r0s17" href="#r0s17">org.apache.pulsar.broker.service.ReplicatorTest</a>
```
✔️ testResumptionAfterBacklogRelaxed
✔️ testReplicationOverrides
✔️ testResetCursorNotFail
✔️ testUpdateGlobalTopicPartition
✔️ testReplication
✔️ testReplicatorOnPartitionedTopic
✔️ testConcurrentReplicator
✔️ testTopicReplicatedAndProducerCreate
✔️ testDeleteReplicatorFailure
✔️ testReplicatorOnPartitionedTopic
✔️ testReplicationForBatchMessages
✔️ testReplicatorClearBacklog
✔️ verifyChecksumAfterReplication
✔️ testCloseReplicatorStartProducer
✔️ activeBrokerParse
✔️ testReplicatePeekAndSkip
✔️ testReplication
✔️ testReplicatedCluster
✔️ testTopicReplicatedAndProducerCreate
✔️ testConfigChange
✔️ testFailures
✔️ testReplicatorProducerClosing
```
### ✔️ <a id="user-content-r0s18" href="#r0s18">org.apache.pulsar.broker.service.TopicOwnerTest</a>
```
✔️ testReleaseOwnershipWithZookeeperDisconnectedBeforeOwnershipNodeDeleted
✔️ testAcquireOwnershipWithZookeeperDisconnectedAfterOwnershipNodeCreated
✔️ testConnectToInvalidateBundleCacheBroker
✔️ testAcquireOwnershipWithZookeeperDisconnectedBeforeOwnershipNodeCreated
✔️ testLookupPartitionedTopic
✔️ testListNonPersistentTopic
✔️ testReleaseOwnershipWithZookeeperDisconnectedAfterOwnershipNodeDeleted
✔️ testReestablishOwnershipAfterInvalidateCache
```
### ✔️ <a id="user-content-r0s19" href="#r0s19">org.apache.pulsar.broker.SLAMonitoringTest</a>
```
✔️ testOwnedNamespaces
✔️ testOwnershipAfterSetup
✔️ testUnloadIfBrokerCrashes
✔️ testOwnershipViaAdminAfterSetup
```
### ✔️ <a id="user-content-r0s20" href="#r0s20">org.apache.pulsar.broker.stats.BookieClientsStatsGeneratorTest</a>
```
✔️ testJvmDirectMemoryUsedMetric
✔️ testBookieClientStatsGenerator
```
### ✔️ <a id="user-content-r0s21" href="#r0s21">org.apache.pulsar.broker.stats.ConsumerStatsTest</a>
```
✔️ testAckStatsOnPartitionedTopicForExclusiveSubscription
✔️ testConsumerStatsOnZeroMaxUnackedMessagesPerConsumer
✔️ testUpdateStatsForActiveConsumerAndSubscription
```
### ✔️ <a id="user-content-r0s22" href="#r0s22">org.apache.pulsar.broker.stats.ManagedCursorMetricsTest</a>
```
✔️ testManagedCursorMetrics
```
### ✔️ <a id="user-content-r0s23" href="#r0s23">org.apache.pulsar.broker.stats.ManagedLedgerMetricsTest</a>
```
✔️ testManagedLedgerMetrics
```
### ✔️ <a id="user-content-r0s24" href="#r0s24">org.apache.pulsar.broker.stats.prometheus.AggregatedNamespaceStatsTest</a>
```
✔️ testSimpleAggregation
```
### ✔️ <a id="user-content-r0s25" href="#r0s25">org.apache.pulsar.broker.stats.PrometheusMetricsTest</a>
```
✔️ testPerTopicStats
✔️ testAuthMetrics
✔️ testPerTopicExpiredStat
✔️ testPerProducerStats
✔️ testMetricsTopicCount
✔️ testManagedLedgerBookieClientStats
✔️ testDuplicateMetricTypeDefinitions
✔️ testExpiringTokenMetrics
✔️ testPerConsumerStats
✔️ testPerNamespaceStats
✔️ testManagedCursorPersistStats
✔️ testDuplicateMetricTypeDefinitions
✔️ testExpiredTokenMetrics
✔️ testManagedLedgerCacheStats
✔️ testManagedLedgerStats
```
### ✔️ <a id="user-content-r0s26" href="#r0s26">org.apache.pulsar.broker.stats.SubscriptionStatsTest</a>
```
✔️ testConsumersAfterMarkDelete
✔️ testNonContiguousDeletedMessagesRanges
```
### ✔️ <a id="user-content-r0s27" href="#r0s27">org.apache.pulsar.broker.systopic.NamespaceEventsSystemTopicServiceTest</a>
```
✔️ testSendAndReceiveNamespaceEvents
```
### ✔️ <a id="user-content-r0s28" href="#r0s28">org.apache.pulsar.broker.transaction.buffer.InMemTransactionBufferReaderTest</a>
```
✔️ testCloseReleaseAllEntries
✔️ testInvalidNumEntriesArgument
✔️ testEndOfTransactionException
```
### ✔️ <a id="user-content-r0s29" href="#r0s29">org.apache.pulsar.broker.transaction.buffer.TransactionBufferClientTest</a>
```
✔️ testAbortOnTopic
✔️ testAbortOnSubscription
✔️ testCommitOnTopic
✔️ testCommitOnSubscription
```
### ✔️ <a id="user-content-r0s30" href="#r0s30">org.apache.pulsar.broker.transaction.buffer.TransactionBufferTest</a>
```
✔️ testOpenReaderOnNonExistentTxn
✔️ testAbortCommittedTxn
✔️ testAbortTxn
✔️ testAbortNonExistentTxn
✔️ testCommitNonExistentTxn
✔️ testCommitTxn
✔️ testOpenReaderOnAnOpenTxn
```
### ✔️ <a id="user-content-r0s31" href="#r0s31">org.apache.pulsar.broker.transaction.buffer.TransactionEntryImplTest</a>
```
✔️ testCloseShouldReleaseBuffer
```
### ✔️ <a id="user-content-r0s32" href="#r0s32">org.apache.pulsar.broker.transaction.buffer.TransactionLowWaterMarkTest</a>
```
✔️ testTransactionBufferLowWaterMark
✔️ testPendingAckLowWaterMark
```
### ✔️ <a id="user-content-r0s33" href="#r0s33">org.apache.pulsar.broker.transaction.buffer.TransactionStablePositionTest</a>
```
✔️ commitTxnTest
✔️ abortTxnTest
✖️ commitTxnTest
```
### ✔️ <a id="user-content-r0s34" href="#r0s34">org.apache.pulsar.broker.transaction.coordinator.TransactionCoordinatorClientTest</a>
```
✔️ testClientStart
✔️ testCommitAndAbort
✔️ testNewTxn
```
### ✔️ <a id="user-content-r0s35" href="#r0s35">org.apache.pulsar.broker.transaction.coordinator.TransactionMetaStoreAssignmentTest</a>
```
✔️ testTransactionMetaStoreAssignAndFailover
```
### ✔️ <a id="user-content-r0s36" href="#r0s36">org.apache.pulsar.broker.transaction.pendingack.PendingAckInMemoryDeleteTest</a>
```
✖️ txnAckTestNoBatchAndSharedSubMemoryDeleteTest
✔️ txnAckTestNoBatchAndSharedSubMemoryDeleteTest
✔️ txnAckTestBatchAndSharedSubMemoryDeleteTest
```
### ✔️ <a id="user-content-r0s37" href="#r0s37">org.apache.pulsar.broker.transaction.TransactionConsumeTest</a>
```
✔️ noSortedTest
✔️ sortedTest
```
### ✔️ <a id="user-content-r0s38" href="#r0s38">org.apache.pulsar.broker.web.RestExceptionTest</a>
```
✔️ testRestException
✔️ testWebApplicationException
✔️ testOtherException
```
### ✔️ <a id="user-content-r0s39" href="#r0s39">org.apache.pulsar.broker.web.WebServiceTest</a>
```
✔️ testTlsAuthDisallowInsecure
✔️ testBrokerReady
✔️ testDefaultClientVersion
✔️ testTlsEnabled
✔️ testTlsAuthAllowInsecure
✔️ testSplitPath
✔️ testMaxRequestSize
✔️ testTlsDisabled
✔️ testRateLimiting
```
### ✔️ <a id="user-content-r0s40" href="#r0s40">org.apache.pulsar.client.impl.AdminApiKeyStoreTlsAuthTest</a>
```
✔️ testAuthorizedUserAsOriginalPrincipal
✔️ testSuperUserCantListNamespaces
✔️ testPersistentList
✔️ testSuperUserCanListTenants
```
### ✔️ <a id="user-content-r0s41" href="#r0s41">org.apache.pulsar.client.impl.BatchMessageIdImplSerializationTest</a>
```
✔️ testSerializationEmpty
✔️ testSerialization1
✔️ testSerializationNull
✔️ testSerialization2
```
### ✔️ <a id="user-content-r0s42" href="#r0s42">org.apache.pulsar.client.impl.BatchMessageIndexAckDisableTest</a>
```
✔️ testBatchMessageIndexAckForExclusiveSubscription
✔️ testBatchMessageIndexAckForSharedSubscription
✔️ testBatchMessageIndexAckForExclusiveSubscription
✔️ testBatchMessageIndexAckForSharedSubscription
```
### ✔️ <a id="user-content-r0s43" href="#r0s43">org.apache.pulsar.client.impl.BatchMessageIndexAckTest</a>
```
✔️ testBatchMessageIndexAckForSharedSubscription
✔️ testBatchMessageIndexAckForSharedSubscription
✔️ testDoNotRecycleAckSetMultipleTimes
✔️ testBatchMessageIndexAckForExclusiveSubscription
✔️ testBatchMessageIndexAckForExclusiveSubscription
```
### ✔️ <a id="user-content-r0s44" href="#r0s44">org.apache.pulsar.client.impl.BrokerClientIntegrationTest</a>
```
✔️ testDisconnectClientWithoutClosingConnection
✔️ testResetCursor
✔️ testResetCursor
✔️ testCloseBrokerService
✔️ testUnsupportedBatchMessageConsumer
✔️ testAvroSchemaProducerConsumerWithSpecifiedReaderAndWriter
✔️ testJsonSchemaProducerConsumerWithSpecifiedReaderAndWriter
✔️ testOperationTimeout
✔️ testCleanProducer
✔️ testUnsupportedBatchMessageConsumer
✔️ testCloseConnectionOnBrokerRejectedRequest
✔️ testAddEntryOperationTimeout
✔️ testInvalidDynamicConfiguration
✔️ testMaxConcurrentTopicLoading
✔️ testCloseConnectionOnInternalServerError
```
### ✔️ <a id="user-content-r0s45" href="#r0s45">org.apache.pulsar.client.impl.CompactedOutBatchMessageTest</a>
```
✔️ testCompactedOutMessages
```
### ✔️ <a id="user-content-r0s46" href="#r0s46">org.apache.pulsar.client.impl.ConsumerAckResponseTest</a>
```
✔️ testAckResponse
```
### ✔️ <a id="user-content-r0s47" href="#r0s47">org.apache.pulsar.client.impl.ConsumerConfigurationTest</a>
```
✔️ testReadCompactNonPersistentExclusive
✔️ testReadCompactPersistentExclusive
✔️ testReadCompactPersistentFailover
✔️ testReadCompactPersistentShared
```
### ✔️ <a id="user-content-r0s48" href="#r0s48">org.apache.pulsar.client.impl.ConsumerDedupPermitsUpdate</a>
```
✔️ testConsumerDedup
✔️ testConsumerDedup
✔️ testConsumerDedup
✔️ testConsumerDedup
✔️ testConsumerDedup
✔️ testConsumerDedup
✔️ testConsumerDedup
```
### ✔️ <a id="user-content-r0s49" href="#r0s49">org.apache.pulsar.client.impl.ConsumerUnsubscribeTest</a>
```
✔️ testConsumerUnsubscribeReference
```
### ✔️ <a id="user-content-r0s50" href="#r0s50">org.apache.pulsar.client.impl.KeyStoreTlsProducerConsumerTestWithAuth</a>
```
✔️ testTlsClientAuthOverHTTPProtocol
✔️ testTlsClientAuthOverBinaryProtocol
✔️ testTlsLargeSizeMessage
```
### ✔️ <a id="user-content-r0s51" href="#r0s51">org.apache.pulsar.client.impl.KeyStoreTlsProducerConsumerTestWithoutAuth</a>
```
✔️ testTlsClientAuthOverHTTPProtocol
✔️ testTlsClientAuthOverBinaryProtocol
✔️ testTlsLargeSizeMessage
```
### ✔️ <a id="user-content-r0s52" href="#r0s52">org.apache.pulsar.client.impl.KeyStoreTlsTest</a>
```
✔️ testValidate
```
### ✔️ <a id="user-content-r0s53" href="#r0s53">org.apache.pulsar.client.impl.MessageChecksumTest</a>
```
✔️ testChecksumCompatibilityInMixedVersionBrokerCluster
✔️ testTamperingMessageIsDetected
✔️ testChecksumCompatibilityInMixedVersionBrokerCluster
```
### ✔️ <a id="user-content-r0s54" href="#r0s54">org.apache.pulsar.client.impl.MessageChunkingTest</a>
```
✔️ testPublishWithFailure
✔️ testInvalidUseCaseForChunking
✔️ testLargeMessage
✔️ testExpireIncompleteChunkMessage
✔️ testInvalidConfig
✔️ testLargeMessageAckTimeOut
✔️ testLargeMessageAckTimeOut
✔️ testLargeMessage
✖️ testMaxPendingChunkMessages
```
### ✔️ <a id="user-content-r0s55" href="#r0s55">org.apache.pulsar.client.impl.MessageParserTest</a>
```
✔️ testWithoutBatches
✔️ testWithBatches
```
### ✔️ <a id="user-content-r0s56" href="#r0s56">org.apache.pulsar.client.impl.MultiTopicsReaderTest</a>
```
✔️ testReadMessageWithBatchingWithMessageInclusive
✔️ testKeyHashRangeReader
✔️ testRemoveSubscriptionForReaderNeedRemoveCursor
✔️ testReadMessageWithBatching
✔️ testReadMessageWithoutBatchingWithMessageInclusive
✔️ testMultiReaderSeek
✔️ testReadMessageWithoutBatching
✔️ testReaderWithTimeLong
```
### ✔️ <a id="user-content-r0s57" href="#r0s57">org.apache.pulsar.client.impl.NegativeAcksTest</a>
```
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
✔️ testNegativeAcks
```
### ✔️ <a id="user-content-r0s58" href="#r0s58">org.apache.pulsar.client.impl.PatternTopicsConsumerImplTest</a>
```
✔️ testStartEmptyPatternConsumer
✔️ testBinaryProtoToGetTopicsOfNamespaceAll
✔️ testPatternTopicsSubscribeWithBuilderFail
✔️ testPubRateOnNonPersistent
✔️ testTopicDeletion
✔️ testAutoUnbubscribePatternConsumer
✔️ testTopicsPatternFilter
✔️ testBinaryProtoToGetTopicsOfNamespaceNonPersistent
✔️ testBinaryProtoToGetTopicsOfNamespacePersistent
✔️ testTopicsListMinus
✔️ testAutoSubscribePatternConsumer
```
### ✔️ <a id="user-content-r0s59" href="#r0s59">org.apache.pulsar.client.impl.PerMessageUnAcknowledgedRedeliveryTest</a>
```
✔️ testSharedAckedNormalTopic
✔️ testUnAckedMessageTrackerSize
✔️ testSharedAckedPartitionedTopic
✔️ testExclusiveAckedNormalTopic
✔️ testFailoverAckedNormalTopic
```
### ✔️ <a id="user-content-r0s60" href="#r0s60">org.apache.pulsar.client.impl.PulsarMultiHostClientTest</a>
```
✔️ testMultiHostUrlRetrySuccess
✔️ testGetPartitionedTopicDataTimeout
✔️ testGetPartitionedTopicMetaData
```
### ✔️ <a id="user-content-r0s61" href="#r0s61">org.apache.pulsar.client.impl.RawMessageSerDeserTest</a>
```
✔️ testSerializationAndDeserialization
```
### ✔️ <a id="user-content-r0s62" href="#r0s62">org.apache.pulsar.client.impl.SchemaDeleteTest</a>
```
✔️ createTopicDeleteTopicCreateTopic
```
### ✔️ <a id="user-content-r0s63" href="#r0s63">org.apache.pulsar.client.impl.SequenceIdWithErrorTest</a>
```
✔️ testCheckSequenceId
✔️ testDeleteTopicWithMissingData
✔️ testTopicWithWildCardChar
✖️ testCrashBrokerWithoutCursorLedgerLeak
✖️ testSkipCorruptDataLedger
```
### ✔️ <a id="user-content-r0s64" href="#r0s64">org.apache.pulsar.client.impl.TopicDoesNotExistsTest</a>
```
✔️ testCreateConsumerOnNotExistsTopic
✔️ testCreateProducerOnNotExistsTopic
```
### ✔️ <a id="user-content-r0s65" href="#r0s65">org.apache.pulsar.client.impl.TopicFromMessageTest</a>
```
✔️ testSingleTopicConsumerNoBatchFullName
✔️ testMultiTopicConsumerBatchShortName
✔️ testSingleTopicConsumerNoBatchShortName
✔️ testMultiTopicConsumerNoBatchShortName
✔️ testSingleTopicConsumerBatchShortName
```
### ✔️ <a id="user-content-r0s66" href="#r0s66">org.apache.pulsar.client.impl.TopicsConsumerImplTest</a>
```
✔️ testTopicAutoUpdatePartitions
✔️ testDifferentTopicsNameSubscribe
✔️ testGetLastMessageId
✔️ testConsumerUnackedRedelivery
✔️ testSubscriptionMustCompleteWhenOperationTimeoutOnMultipleTopics
✔️ testConsumerDistributionInFailoverSubscriptionWhenUpdatePartitions
✔️ multiTopicsInDifferentNameSpace
✔️ testDefaultBacklogTTL
✔️ testGetConsumersAndGetTopics
✔️ testSubscribeUnsubscribeSingleTopic
✔️ testResubscribeSameTopic
✔️ testSyncProducerAndConsumer
✔️ testPartitionsUpdatesForMultipleTopics
✔️ testTopicsNameSubscribeWithBuilderFail
✔️ testMultiTopicsMessageListener
✔️ testTopicNameValid
✔️ testAsyncConsumer
```
### ✔️ <a id="user-content-r0s67" href="#r0s67">org.apache.pulsar.client.impl.UnAcknowledgedMessagesTimeoutTest</a>
```
✔️ testCheckUnAcknowledgedMessageTimer
✔️ testExclusiveSingleAckedNormalTopic
✔️ testFailoverSingleAckedPartitionedTopic
✔️ testSharedSingleAckedPartitionedTopic
✔️ testAckTimeoutMinValue
✔️ testExclusiveCumulativeAckedNormalTopic
✔️ testSingleMessageBatch
```
### ✔️ <a id="user-content-r0s68" href="#r0s68">org.apache.pulsar.client.impl.ZeroQueueSizeTest</a>
```
✔️ zeroQueueSizeSharedSubscription
✔️ testPauseAndResume
✔️ testZeroQueueSizeMessageRedeliveryForAsyncReceive
✔️ zeroQueueSizeConsumerListener
✔️ zeroQueueSizeFailoverSubscription
✔️ validQueueSizeConfig
✔️ zeroQueueSizeNormalConsumer
✔️ zeroQueueSizeReceieveAsyncInCompatibility
✔️ InvalidQueueSizeConfig
✔️ testZeroQueueSizeMessageRedeliveryForListener
✔️ testZeroQueueSizeMessageRedelivery
✔️ zeroQueueSizePartitionedTopicInCompatibility
✔️ testFailedZeroQueueSizeBatchMessage
✔️ testPauseAndResumeWithUnloading
```
### ✔️ <a id="user-content-r0s69" href="#r0s69">org.apache.pulsar.common.api.raw.RawMessageImplTest</a>
```
✔️ testGetProperties
```
### ✔️ <a id="user-content-r0s70" href="#r0s70">org.apache.pulsar.common.compression.CommandsTest</a>
```
✔️ testChecksumSendCommand
```
### ✔️ <a id="user-content-r0s71" href="#r0s71">org.apache.pulsar.common.compression.CompressorCodecBackwardCompatTest</a>
```
✔️ testCompressDecompress
✔️ testCompressDecompress
✔️ testCompressDecompress
✔️ testCompressDecompress
✔️ testCompressDecompress
✔️ testCompressDecompress
```
### ✔️ <a id="user-content-r0s72" href="#r0s72">org.apache.pulsar.common.compression.CompressorCodecTest</a>
```
✔️ testCompressDecompress
✔️ testMultpileUsages
✔️ testMultpileUsages
✔️ testCompressDecompress
✔️ testMultpileUsages
✔️ testCompressDecompress
✔️ testMultpileUsages
✔️ testCompressDecompress
✔️ testDecompressFromSampleBuffer
✔️ testDecompressReadonlyByteBuf
✔️ testDecompressReadonlyByteBuf
✔️ testCodecProvider
✔️ testEmptyInput
✔️ testEmptyInput
✔️ testCompressDecompress
✔️ testCodecProvider
✔️ testDecompressFromSampleBuffer
✔️ testMultpileUsages
✔️ testCodecProvider
✔️ testEmptyInput
✔️ testDecompressReadonlyByteBuf
✔️ testCompressDecompress
✔️ testDecompressReadonlyByteBuf
✔️ testCompressDecompress
✔️ testCompressDecompress
✔️ testMultpileUsages
✔️ testEmptyInput
✔️ testDecompressReadonlyByteBuf
✔️ testDecompressFromSampleBuffer
✔️ testDecompressFromSampleBuffer
✔️ testDecompressFromSampleBuffer
✔️ testDecompressReadonlyByteBuf
✔️ testDecompressReadonlyByteBuf
✔️ testMultpileUsages
✔️ testCompressDecompress
✔️ testCodecProvider
✔️ testMultpileUsages
✔️ testCompressDecompress
✔️ testMultpileUsages
✔️ testDecompressReadonlyByteBuf
✔️ testEmptyInput
✔️ testCodecProvider
✔️ testDecompressReadonlyByteBuf
✔️ testDecompressReadonlyByteBuf
✔️ testMultpileUsages
```
### ✔️ <a id="user-content-r0s73" href="#r0s73">org.apache.pulsar.common.compression.Crc32cChecksumTest</a>
```
✔️ testCrc32cHardware
✔️ testCrc32cDirectMemoryHardware
✔️ testCrc32c
✔️ testCrc32cSoftware
✔️ testCrc32cIncremental
✔️ testCrc32cIncrementalUsingProvider
```
### ✔️ <a id="user-content-r0s74" href="#r0s74">org.apache.pulsar.common.lookup.data.LookupDataTest</a>
```
✔️ testLoadReportSerialization
✔️ testUrlEncoder
✔️ serializeToJsonTest
✔️ withConstructor
```
### ✔️ <a id="user-content-r0s75" href="#r0s75">org.apache.pulsar.common.naming.MetadataTests</a>
```
✔️ testInvalidMetadata
✔️ testValidMetadata
```
### ✔️ <a id="user-content-r0s76" href="#r0s76">org.apache.pulsar.common.naming.NamespaceBundlesTest</a>
```
✔️ testConstructor
✔️ testSplitBundleInTwo
✔️ testsplitBundles
✔️ testFindBundle
✔️ testSplitBundleByFixBoundary
```
### ✔️ <a id="user-content-r0s77" href="#r0s77">org.apache.pulsar.common.naming.NamespaceBundleTest</a>
```
✔️ testIncludes
✔️ testGetBundle
✔️ testCompareTo
✔️ testConstructor
✔️ testToString
✔️ testEquals
```
### ✔️ <a id="user-content-r0s78" href="#r0s78">org.apache.pulsar.common.naming.NamespaceNameTest</a>
```
✔️ namespace
✔️ testNewScheme
```
### ✔️ <a id="user-content-r0s79" href="#r0s79">org.apache.pulsar.common.naming.ServiceConfigurationTest</a>
```
✔️ testOptionalSettingPresent
✔️ testOptionalSettingEmpty
✔️ testInit
✔️ testInitFailure
```
### ✔️ <a id="user-content-r0s80" href="#r0s80">org.apache.pulsar.common.naming.TopicNameTest</a>
```
✔️ testShortTopicName
✔️ topic
✔️ testTopicNameWithoutCluster
✔️ testDecodeEncode
```
### ✔️ <a id="user-content-r0s81" href="#r0s81">org.apache.pulsar.common.net.ServiceURITest</a>
```
✔️ testEmptyServiceUriString
✔️ testMultipleHostsSemiColon
✔️ testInvalidServiceUris
✔️ testMultipleHostsWithoutHttpPorts
✔️ testRootPath
✔️ testMultipleHostsMixedPorts
✔️ testMultipleHostsWithoutPulsarTlsPorts
✔️ testUserInfoWithMultipleHosts
✔️ testMultipleHostsComma
✔️ testMultipleHostsMixed
✔️ testUserInfo
✔️ testIpv6UriWithoutPulsarPort
✔️ testMultiIpv6Uri
✔️ testMultiIpv6UriWithoutPulsarPort
✔️ testEmptyPath
✔️ testNullServiceUriString
✔️ testNullServiceUriInstance
✔️ testMissingServiceName
✔️ testMultipleHostsWithoutHttpsPorts
✔️ testMultipleHostsWithoutPulsarPorts
✔️ testIpv6Uri
```
### ✔️ <a id="user-content-r0s82" href="#r0s82">org.apache.pulsar.common.policies.data.AutoFailoverPolicyDataTest</a>
```
✔️ testAutoFailoverPolicyData
```
### ✔️ <a id="user-content-r0s83" href="#r0s83">org.apache.pulsar.common.policies.data.AutoFailoverPolicyTypeTest</a>
```
✔️ testAutoFailoverPolicyType
```
### ✔️ <a id="user-content-r0s84" href="#r0s84">org.apache.pulsar.common.policies.data.AutoTopicCreationOverrideTest</a>
```
✔️ testInvalidTopicType
✔️ testNumPartitionsTooLow
✔️ testNumPartitionsNotSet
✔️ testValidOverrideNonPartitioned
✔️ testNumPartitionsOnNonPartitioned
✔️ testValidOverridePartitioned
```
### ✔️ <a id="user-content-r0s85" href="#r0s85">org.apache.pulsar.common.policies.data.BacklogQuotaTest</a>
```
✔️ testBacklogQuotaIdentity
```
### ✔️ <a id="user-content-r0s86" href="#r0s86">org.apache.pulsar.common.policies.data.ClusterDataTest</a>
```
✔️ simple
```
### ✔️ <a id="user-content-r0s87" href="#r0s87">org.apache.pulsar.common.policies.data.ConsumerStatsTest</a>
```
✔️ testConsumerStats
```
### ✔️ <a id="user-content-r0s88" href="#r0s88">org.apache.pulsar.common.policies.data.EnsemblePlacementPolicyConfigTest</a>
```
✔️ testDecodeFailed
✔️ testEncodeDecodeSuccessfully
```
### ✔️ <a id="user-content-r0s89" href="#r0s89">org.apache.pulsar.common.policies.data.LocalPolicesTest</a>
```
✔️ testLocalPolices
```
### ✔️ <a id="user-content-r0s90" href="#r0s90">org.apache.pulsar.common.policies.data.NamespaceIsolationDataTest</a>
```
✔️ testNamespaceIsolationData
```
### ✔️ <a id="user-content-r0s91" href="#r0s91">org.apache.pulsar.common.policies.data.NamespaceOwnershipStatusTest</a>
```
✔️ testSerialization
```
### ✔️ <a id="user-content-r0s92" href="#r0s92">org.apache.pulsar.common.policies.data.OffloadPoliciesTest</a>
```
✔️ testGcsConfiguration
✔️ mergeTest
✔️ compatibleWithConfigFileTest
✔️ testCreateByProperties
✔️ testS3Configuration
✔️ oldPoliciesCompatibleTest
```
### ✔️ <a id="user-content-r0s93" href="#r0s93">org.apache.pulsar.common.policies.data.PartitionedTopicStatsTest</a>
```
✔️ testPartitionedTopicStats
```
### ✔️ <a id="user-content-r0s94" href="#r0s94">org.apache.pulsar.common.policies.data.PersistencePoliciesTest</a>
```
✔️ testPersistencePolicies
```
### ✔️ <a id="user-content-r0s95" href="#r0s95">org.apache.pulsar.common.policies.data.PersistentOfflineTopicStatsTest</a>
```
✔️ testPersistentOfflineTopicStats
```
### ✔️ <a id="user-content-r0s96" href="#r0s96">org.apache.pulsar.common.policies.data.PersistentTopicStatsTest</a>
```
✔️ testPersistentTopicStatsAggregation
✔️ testPersistentTopicStats
```
### ✔️ <a id="user-content-r0s97" href="#r0s97">org.apache.pulsar.common.policies.data.PoliciesDataTest</a>
```
✔️ propertyAdmin
✔️ policies
✔️ bundlesData
✔️ bundlesPolicies
```
### ✔️ <a id="user-content-r0s98" href="#r0s98">org.apache.pulsar.common.policies.data.PublisherStatsTest</a>
```
✔️ testPublisherStats
✔️ testPublisherStatsAggregation
```
### ✔️ <a id="user-content-r0s99" href="#r0s99">org.apache.pulsar.common.policies.data.ReplicatorStatsTest</a>
```
✔️ testReplicatorStatsAdd
✔️ testReplicatorStatsNull
```
### ✔️ <a id="user-content-r0s100" href="#r0s100">org.apache.pulsar.common.policies.data.ResourceQuotaTest</a>
```
✔️ testResourceQuotaDefault
✔️ testResourceQuotaEqual
```
### ✔️ <a id="user-content-r0s101" href="#r0s101">org.apache.pulsar.common.policies.data.RetentionPolicesTest</a>
```
✔️ testRetentionPolices
```
### ✔️ <a id="user-content-r0s102" href="#r0s102">org.apache.pulsar.common.policies.impl.AutoFailoverPolicyFactoryTest</a>
```
✔️ testAutoFailoverPolicyFactory
```
### ✔️ <a id="user-content-r0s103" href="#r0s103">org.apache.pulsar.common.policies.impl.MinAvailablePolicyTest</a>
```
✔️ testMinAvailablePolicty
```
### ✔️ <a id="user-content-r0s104" href="#r0s104">org.apache.pulsar.common.policies.impl.NamespaceIsolationPoliciesTest</a>
```
✔️ testBrokerAssignment
✔️ testGetNamespaceIsolationPolicyByName
✔️ testDeletePolicy
✔️ testSetPolicy
✔️ testJsonSerialization
✔️ testDefaultConstructor
✔️ testGetNamespaceIsolationPolicyByNamespace
```
### ✔️ <a id="user-content-r0s105" href="#r0s105">org.apache.pulsar.common.policies.impl.NamespaceIsolationPolicyImplTest</a>
```
✔️ testFindBrokers
✔️ testGetSecondaryBrokers
✔️ testShouldFailover
✔️ testGetPrimaryBrokers
✔️ testGetAvailablePrimaryBrokers
✔️ testConstructor
✔️ testIsPrimaryOrSecondaryBroker
```
### ✔️ <a id="user-content-r0s106" href="#r0s106">org.apache.pulsar.common.protocol.ByteBufPairTest</a>
```
✔️ testEncoder
✔️ testDoubleByteBuf
```
### ✔️ <a id="user-content-r0s107" href="#r0s107">org.apache.pulsar.common.protocol.CommandUtilsTests</a>
```
✔️ testSkipBrokerEntryMetadata
✔️ testPeekBrokerEntryMetadata
✔️ testParseBrokerEntryMetadata
✔️ testMetadataFromCommandSubscribe
✔️ testMetadataFromCommandProducer
✔️ testAddBrokerEntryMetadata
✔️ testByteBufComposite
```
### ✔️ <a id="user-content-r0s108" href="#r0s108">org.apache.pulsar.common.protocol.MarkersTest</a>
```
✔️ testSnapshot
✔️ testTxnAbortMarker
✔️ testUpdate
✔️ testTxnCommitMarker
✔️ testSnapshotRequest
✔️ testSnapshotResponse
```
### ✔️ <a id="user-content-r0s109" href="#r0s109">org.apache.pulsar.common.protocol.PulsarDecoderTest</a>
```
✔️ testChannelRead
```
### ✔️ <a id="user-content-r0s110" href="#r0s110">org.apache.pulsar.common.stats.JvmDefaultGCMetricsLoggerTest</a>
```
✔️ testInvokeJVMInternals
```
### ✔️ <a id="user-content-r0s111" href="#r0s111">org.apache.pulsar.common.util.collections.BitSetRecyclableRecyclableTest</a>
```
✔️ testResetWords
✔️ testRecycle
```
### ✔️ <a id="user-content-r0s112" href="#r0s112">org.apache.pulsar.common.util.collections.ConcurrentBitSetRecyclableTest</a>
```
✔️ testRecycle
✔️ testGenerateByBitSet
```
### ✔️ <a id="user-content-r0s113" href="#r0s113">org.apache.pulsar.common.util.collections.ConcurrentLongHashMapTest</a>
```
✔️ testRehashingWithDeletes
✔️ concurrentInsertionsAndReads
✔️ testRemove
✔️ testRehashing
✔️ simpleInsertions
✔️ testComputeIfAbsent
✔️ testConstructor
✔️ testPutIfAbsent
✔️ testIteration
✔️ testHashConflictWithDeletion
✔️ concurrentInsertions
✔️ stressConcurrentInsertionsAndReads
✔️ testNegativeUsedBucketCount
```
### ✔️ <a id="user-content-r0s114" href="#r0s114">org.apache.pulsar.common.util.collections.ConcurrentLongPairSetTest</a>
```
✔️ concurrentInsertionsAndReads
✔️ testEqualsObjects
✔️ testIfRemoval
✔️ testRehashing
✔️ testToString
✔️ testRemove
✔️ testItems
✔️ testRehashingWithDeletes
✔️ testHashConflictWithDeletion
✔️ testIteration
✔️ simpleInsertions
✔️ testRehashingRemoval
✔️ testRemoval
✔️ testConstructor
✔️ concurrentInsertions
```
### ✔️ <a id="user-content-r0s115" href="#r0s115">org.apache.pulsar.common.util.collections.ConcurrentOpenHashMapTest</a>
```
✔️ testRemove
✔️ simpleInsertions
✔️ testPutIfAbsent
✔️ concurrentInsertions
✔️ testHashConflictWithDeletion
✔️ testRehashingWithDeletes
✔️ testComputeIfAbsent
✔️ testRehashing
✔️ testIteration
✔️ testEqualsKeys
✔️ concurrentInsertionsAndReads
✔️ testConstructor
```
### ✔️ <a id="user-content-r0s116" href="#r0s116">org.apache.pulsar.common.util.collections.ConcurrentOpenHashSetTest</a>
```
✔️ concurrentInsertions
✔️ testRehashing
✔️ testRemoval
✔️ testEqualsObjects
✔️ testHashConflictWithDeletion
✔️ testConstructor
✔️ concurrentInsertionsAndReads
✔️ testIteration
✔️ simpleInsertions
✔️ testRehashingWithDeletes
✔️ testRemove
```
### ✔️ <a id="user-content-r0s117" href="#r0s117">org.apache.pulsar.common.util.collections.ConcurrentOpenLongPairRangeSetTest</a>
```
✔️ testAddForDifferentKey
✔️ testToString
✔️ testCacheFlagConflict
✔️ testDeleteWithLeastMost
✔️ testDeleteForDifferentKey
✔️ testLastRange
✔️ testAddCompareCompareWithGuava
✔️ testSpanWithGuava
✔️ testDeleteCompareWithGuava
✔️ testFirstRange
✔️ testAddForSameKey
✔️ testDeleteWithAtMost
✔️ testRangeContaining
```
### ✔️ <a id="user-content-r0s118" href="#r0s118">org.apache.pulsar.common.util.collections.ConcurrentSortedLongPairSetTest</a>
```
✔️ concurrentInsertions
✔️ testIfRemoval
✔️ testRemoval
✔️ testRemove
✔️ testItems
✔️ testEqualsObjects
✔️ simpleInsertions
✔️ testIteration
✔️ testToString
```
### ✔️ <a id="user-content-r0s119" href="#r0s119">org.apache.pulsar.common.util.collections.FieldParserTest</a>
```
✔️ testUpdateObject
✔️ testConversion
```
### ✔️ <a id="user-content-r0s120" href="#r0s120">org.apache.pulsar.common.util.collections.GrowableArrayBlockingQueueTest</a>
```
✔️ removeTest
✔️ growArray
✔️ pollTimeout
✔️ simple
✔️ pollTimeout2
✔️ blockingTake
```
### ✔️ <a id="user-content-r0s121" href="#r0s121">org.apache.pulsar.common.util.collections.GrowablePriorityLongPairQueueTest</a>
```
✔️ testItems
✔️ testRemove
✔️ testExpandQueue
✔️ testInsertAndRemove
✔️ testEqualsObjects
✔️ testExpandRemoval
✔️ testIteration
✔️ simpleInsertions
✔️ concurrentInsertions
✔️ testConstructor
✔️ testSetWithDuplicateInsert
✔️ testExpandWithDeletes
✔️ concurrentInsertionsAndReads
✔️ testRemoval
✔️ testIfRemoval
```
### ✔️ <a id="user-content-r0s122" href="#r0s122">org.apache.pulsar.common.util.collections.TripleLongPriorityQueueTest</a>
```
✔️ testQueue
✔️ testCheckForEmpty
✔️ testCompareWithSamePrefix
```
### ✔️ <a id="user-content-r0s123" href="#r0s123">org.apache.pulsar.common.util.FieldParserTest</a>
```
✔️ testMap
```
### ✔️ <a id="user-content-r0s124" href="#r0s124">org.apache.pulsar.common.util.FileModifiedTimeUpdaterTest</a>
```
✔️ testFileNotModified
✔️ testFileModified
✔️ testFileModified
✔️ testFileNotModified
✔️ testFileModified
✔️ testFileNotModified
```
### ✔️ <a id="user-content-r0s125" href="#r0s125">org.apache.pulsar.common.util.netty.ChannelFuturesTest</a>
```
✔️ toCompletableFuture_shouldCompleteExceptionally_channelFutureCompletedAfter
✔️ toCompletableFuture_shouldCompleteSuccessfully_channelFutureCompletedAfter
✔️ toCompletableFuture_shouldCompleteSuccessfully_channelFutureCompletedBefore
✔️ toCompletableFuture_shouldCompleteExceptionally_channelFutureCompletedBefore
✔️ toCompletableFuture_shouldRequireNonNullArgument
```
### ✔️ <a id="user-content-r0s126" href="#r0s126">org.apache.pulsar.common.util.RateLimiterTest</a>
```
✔️ testMultipleTryAcquire
✔️ testRateLimiterWithPermitUpdater
✔️ testTryAcquire
✔️ testTryAcquireNoPermits
✔️ testClose
✔️ testResetRate
✔️ testMultipleAcquire
✔️ testAcquire
✔️ testInvalidRenewTime
✔️ testRateLimiterWithFunction
✔️ testAcquireBlock
```
### ✔️ <a id="user-content-r0s127" href="#r0s127">org.apache.pulsar.common.util.ReflectionsTest</a>
```
✔️ testCreateInstanceNoNoArgConstructor
✔️ testCreateInstanceConstructorThrowsException
✔️ testCreateInstanceAbstractClass
✔️ testCreateTypedInstanceUnassignableClass
✔️ testCreateInstanceClassNotFound
✔️ testCreateTypedInstanceConstructorThrowsException
✔️ testClassExists
✔️ testCreateTypedInstanceAbstractClass
✔️ testCreateTypedInstanceClassNotFound
✔️ testCreateTypedInstanceNoNoArgConstructor
✔️ testLoadClass
✔️ testClassInJarImplementsIface
```
### ✔️ <a id="user-content-r0s128" href="#r0s128">org.apache.pulsar.common.util.RelativeTimeUtilTest</a>
```
✔️ testParseRelativeTime
```
### ✔️ <a id="user-content-r0s129" href="#r0s129">org.apache.pulsar.discovery.service.web.DiscoveryServiceWebTest</a>
```
✔️ testRedirectUrlWithServerStarted
```
### ✔️ <a id="user-content-r0s130" href="#r0s130">org.apache.pulsar.functions.worker.PulsarFunctionE2ESecurityTest</a>
```
✔️ testAuthorizationWithAnonymousUser
✔️ testAuthorization
```
### ✔️ <a id="user-content-r0s131" href="#r0s131">org.apache.pulsar.functions.worker.PulsarFunctionPublishTest</a>
```
✔️ testPulsarFunctionState
✔️ testMultipleAddress
✔️ testPulsarFunctionBKCleanup
```
### ✔️ <a id="user-content-r0s132" href="#r0s132">org.apache.pulsar.functions.worker.PulsarFunctionTlsTest</a>
```
✔️ testFunctionsCreation
```
### ✔️ <a id="user-content-r0s133" href="#r0s133">org.apache.pulsar.io.PulsarFunctionTlsTest</a>
```
✔️ testAuthorization
```
### ✔️ <a id="user-content-r0s134" href="#r0s134">org.apache.pulsar.proxy.server.AdminProxyHandlerTest</a>
```
✔️ replayableProxyContentProviderTest
```
### ✔️ <a id="user-content-r0s135" href="#r0s135">org.apache.pulsar.proxy.server.AuthedAdminProxyHandlerTest</a>
```
✔️ testAuthenticatedProxyAsNonAdmin
```
### ✔️ <a id="user-content-r0s136" href="#r0s136">org.apache.pulsar.proxy.server.FunctionWorkerRoutingTest</a>
```
✔️ testFunctionWorkerRedirect
```
### ✔️ <a id="user-content-r0s137" href="#r0s137">org.apache.pulsar.proxy.server.ProxyAdditionalServletTest</a>
```
✔️ test
```
### ✔️ <a id="user-content-r0s138" href="#r0s138">org.apache.pulsar.proxy.server.ProxyAuthenticatedProducerConsumerTest</a>
```
✔️ testTlsSyncProducerAndConsumer
```
### ✔️ <a id="user-content-r0s139" href="#r0s139">org.apache.pulsar.proxy.server.ProxyAuthenticationTest</a>
```
✔️ testAuthentication
```
### ✔️ <a id="user-content-r0s140" href="#r0s140">org.apache.pulsar.proxy.server.ProxyConnectionThrottlingTest</a>
```
✔️ testInboundConnection
```
### ✔️ <a id="user-content-r0s141" href="#r0s141">org.apache.pulsar.proxy.server.ProxyEnableHAProxyProtocolTest</a>
```
✔️ testSimpleProduceAndConsume
```
### ✔️ <a id="user-content-r0s142" href="#r0s142">org.apache.pulsar.proxy.server.ProxyForwardAuthDataTest</a>
```
✔️ testForwardAuthData
```
### ✔️ <a id="user-content-r0s143" href="#r0s143">org.apache.pulsar.proxy.server.ProxyIsAHttpProxyTest</a>
```
✔️ testProxyToEndsInSlash
✔️ testStreaming
✔️ testLongPath
✔️ testLongPathInProxyTo
✔️ testPathEndsInSlash
✔️ testPathNotSpecified
✔️ testTryingToUseExistingPath
✔️ testMultipleRedirect
✔️ testSingleRedirect
✔️ testRedirectNotSpecified
```
### ✔️ <a id="user-content-r0s144" href="#r0s144">org.apache.pulsar.proxy.server.ProxyKeyStoreTlsTestWithAuth</a>
```
✔️ testProducerFailed
✔️ testPartitions
✔️ testProducer
```
### ✔️ <a id="user-content-r0s145" href="#r0s145">org.apache.pulsar.proxy.server.ProxyKeyStoreTlsTestWithoutAuth</a>
```
✔️ testPartitions
✔️ testProducerFailed
✔️ testProducer
```
### ✔️ <a id="user-content-r0s146" href="#r0s146">org.apache.pulsar.proxy.server.ProxyLookupThrottlingTest</a>
```
✔️ testLookup
```
### ✔️ <a id="user-content-r0s147" href="#r0s147">org.apache.pulsar.proxy.server.ProxyParserTest</a>
```
✔️ testRegexSubscription
✔️ testProducerConsumer
✔️ testProducer
✔️ testPartitions
✔️ testProtocolVersionAdvertisement
```
### ✔️ <a id="user-content-r0s148" href="#r0s148">org.apache.pulsar.proxy.server.ProxyRolesEnforcementTest</a>
```
✔️ testIncorrectRoles
```
### ✔️ <a id="user-content-r0s149" href="#r0s149">org.apache.pulsar.proxy.server.ProxyStatsTest</a>
```
✔️ testChangeLogLevel
✔️ testConnectionsStats
✔️ testTopicStats
```
### ✔️ <a id="user-content-r0s150" href="#r0s150">org.apache.pulsar.proxy.server.ProxyTest</a>
```
✔️ testPartitions
✔️ testRegexSubscription
✔️ testProtocolVersionAdvertisement
✔️ testGetSchema
✔️ testProducer
✔️ testProducerConsumer
```
### ✔️ <a id="user-content-r0s151" href="#r0s151">org.apache.pulsar.proxy.server.ProxyTlsTest</a>
```
✔️ testProducer
✔️ testPartitions
```
### ✔️ <a id="user-content-r0s152" href="#r0s152">org.apache.pulsar.proxy.server.ProxyTlsTestWithAuth</a>
```
✔️ testServiceStartup
```
### ✔️ <a id="user-content-r0s153" href="#r0s153">org.apache.pulsar.proxy.server.ProxyWithAuthorizationNegTest</a>
```
✔️ testProxyAuthorization
```
### ✔️ <a id="user-content-r0s154" href="#r0s154">org.apache.pulsar.proxy.server.ProxyWithAuthorizationTest</a>
```
✔️ tlsCiphersAndProtocols
✔️ testTlsHostVerificationProxyToClient
✔️ tlsCiphersAndProtocols
✔️ testProxyAuthorization
✔️ tlsCiphersAndProtocols
✔️ testTlsHostVerificationProxyToBroker
✔️ tlsCiphersAndProtocols
✔️ tlsCiphersAndProtocols
✔️ tlsCiphersAndProtocols
✔️ testTlsHostVerificationProxyToBroker
✔️ tlsCiphersAndProtocols
✔️ testTlsHostVerificationProxyToClient
✔️ tlsCiphersAndProtocols
```
### ✔️ <a id="user-content-r0s155" href="#r0s155">org.apache.pulsar.proxy.server.ProxyWithoutServiceDiscoveryTest</a>
```
✔️ testDiscoveryService
```
### ✔️ <a id="user-content-r0s156" href="#r0s156">org.apache.pulsar.proxy.server.SuperUserAuthedAdminProxyHandlerTest</a>
```
✔️ testAuthWithRandoCert
✔️ testAuthenticatedProxyAsAdmin
✔️ testAuthenticatedProxyAsNonAdmin
```
### ✔️ <a id="user-content-r0s157" href="#r0s157">org.apache.pulsar.proxy.server.UnauthedAdminProxyHandlerTest</a>
```
✔️ testUnauthenticatedProxy
✔️ testVipStatus
```
### ✔️ <a id="user-content-r0s158" href="#r0s158">org.apache.pulsar.PulsarBrokerStarterTest</a>
```
✔️ testMainRunBookieNoConfig
✔️ testLoadConfigWithException
✔️ testMainWithNoArgument
✔️ testLoadBalancerConfig
✔️ testGlobalZooKeeperConfig
✔️ testMainRunBookieRecoveryNoConfig
✔️ testLoadConfig
✔️ testMainEnableRunBookieThroughBrokerConfig
✔️ testMainRunBookieAndAutoRecoveryNoConfig
```
### ✔️ <a id="user-content-r0s159" href="#r0s159">org.apache.pulsar.schema.compatibility.SchemaCompatibilityCheckTest</a>
```
✔️ testConsumerCompatibilityCheckCanReadLastTest
✔️ testConsumerWithNotCompatibilitySchema
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testConsumerCompatibilityCheckCanReadLastTest
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testSchemaComparison
✔️ testConsumerCompatibilityCheckCanReadLastTest
✔️ testConsumerCompatibilityReadAllCheckTest
✔️ testConsumerWithNotCompatibilitySchema
✔️ testIsAutoUpdateSchema
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testConsumerCompatibilityReadAllCheckTest
✔️ testIsAutoUpdateSchema
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testConsumerWithNotCompatibilitySchema
✔️ testIsAutoUpdateSchema
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testConsumerWithNotCompatibilitySchema
✔️ testProducerSendWithOldSchemaAndConsumerCanRead
✔️ testIsAutoUpdateSchema
✔️ testIsAutoUpdateSchema
✔️ testConsumerCompatibilityCheckCanReadLastTest
✔️ testIsAutoUpdateSchema
```
### ✔️ <a id="user-content-r0s160" href="#r0s160">org.apache.pulsar.schema.PartitionedTopicSchemaTest</a>
```
✔️ test
```
### ✔️ <a id="user-content-r0s161" href="#r0s161">org.apache.pulsar.schema.SchemaTest</a>
```
✔️ testIsUsingAvroSchemaParser
✔️ testBytesSchemaDeserialize
✔️ testMultiTopicSetSchemaProvider
```
### ✔️ <a id="user-content-r0s162" href="#r0s162">org.apache.pulsar.stats.client.PulsarBrokerStatsClientTest</a>
```
✔️ testServiceException
✔️ testTopicInternalStats
```
### ✔️ <a id="user-content-r0s163" href="#r0s163">org.apache.pulsar.tests.EnumValuesDataProviderTest</a>
```
✔️ shouldFailIfEnumParameterIsMissing
✔️ testEnumValuesProvider
✔️ testEnumValuesProvider
✔️ shouldDetermineEnumValuesFromMethod
✔️ shouldContainAllEnumValues
✔️ testEnumValuesProvider
```
### ✔️ <a id="user-content-r0s164" href="#r0s164">org.apache.pulsar.tests.TestRetrySupportBeforeMethodRetryTest</a>
```
✔️ shouldNotDoAnythingWhenThereIsBeforeAndAfterMethod
✖️ shouldNotDoAnythingWhenThereIsBeforeAndAfterMethod
✖️ shouldNotDoAnythingWhenThereIsBeforeAndAfterMethod
✖️ shouldNotDoAnythingWhenThereIsBeforeAndAfterMethod
✖️ shouldNotDoAnythingWhenThereIsBeforeAndAfterMethod
```
### ✔️ <a id="user-content-r0s165" href="#r0s165">org.apache.pulsar.tests.TestRetrySupportRetryTest</a>
```
✖️ shouldCallSetupBeforeRetrying
✔️ shouldCallSetupBeforeRetrying
✖️ shouldCallSetupBeforeRetrying
✖️ shouldCallSetupBeforeRetrying
✖️ shouldCallSetupBeforeRetrying
```
### ✔️ <a id="user-content-r0s166" href="#r0s166">org.apache.pulsar.tests.TestRetrySupportSuccessTest</a>
```
✔️ shouldCallSetupOnce1
✔️ shouldCallSetupOnce3
✔️ shouldCallSetupOnce2
```
### ✔️ <a id="user-content-r0s167" href="#r0s167">org.apache.pulsar.tests.ThreadDumpUtilTest</a>
```
✔️ testHelp
✔️ testThreadDump
```
### ✔️ <a id="user-content-r0s168" href="#r0s168">org.apache.pulsar.utils.SimpleTextOutputStreamTest</a>
```
✔️ testBooleanFormat
✔️ testDoubleFormat
✔️ testLongFormat
✔️ testString
```
### ✔️ <a id="user-content-r0s169" href="#r0s169">org.apache.pulsar.utils.StatsOutputStreamTest</a>
```
✔️ testLists
✔️ testNamedObjects
✔️ testNestedObjects
✔️ testNamedLists
✔️ testPairs
✔️ testObjects
```
### ✔️ <a id="user-content-r0s170" href="#r0s170">org.apache.pulsar.websocket.proxy.ProxyAuthenticationTest</a>
```
✔️ unauthenticatedSocketTest
✔️ authenticatedSocketTest
✔️ statsTest
✔️ anonymousSocketTest
```
### ✔️ <a id="user-content-r0s171" href="#r0s171">org.apache.pulsar.websocket.proxy.ProxyAuthorizationTest</a>
```
✔️ test
```
### ✔️ <a id="user-content-r0s172" href="#r0s172">org.apache.pulsar.websocket.proxy.ProxyConfigurationTest</a>
```
✔️ configTest
✔️ configTest
```
### ✔️ <a id="user-content-r0s173" href="#r0s173">org.apache.pulsar.websocket.proxy.ProxyPublishConsumeTlsTest</a>
```
✔️ socketTest
```
### ✔️ <a id="user-content-r0s174" href="#r0s174">org.apache.pulsar.websocket.proxy.ProxyPublishConsumeWithoutZKTest</a>
```
✔️ socketTest
```
### ✔️ <a id="user-content-r0s175" href="#r0s175">org.apache.pulsar.websocket.proxy.v1.V1_ProxyAuthenticationTest</a>
```
✔️ anonymousSocketTest
✔️ authenticatedSocketTest
✔️ statsTest
✔️ unauthenticatedSocketTest
```