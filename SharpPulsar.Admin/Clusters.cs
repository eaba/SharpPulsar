﻿using System.Collections.Generic;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace Org.Apache.Pulsar.Client.Admin
{

	using ConflictException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.ConflictException;
	using NotAuthorizedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotFoundException;
	using PreconditionFailedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.PreconditionFailedException;
	using BrokerNamespaceIsolationData = Org.Apache.Pulsar.Common.Policies.Data.BrokerNamespaceIsolationData;
	using ClusterData = Org.Apache.Pulsar.Common.Policies.Data.ClusterData;
	using FailureDomain = Org.Apache.Pulsar.Common.Policies.Data.FailureDomain;
	using NamespaceIsolationData = Org.Apache.Pulsar.Common.Policies.Data.NamespaceIsolationData;

	/// <summary>
	/// Admin interface for clusters management.
	/// </summary>
	public interface Clusters
	{
		/// <summary>
		/// Get the list of clusters.
		/// <para>
		/// Get the list of all the Pulsar clusters.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["c1", "c2", "c3"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getClusters() throws PulsarAdminException;
		IList<string> GetClusters();

		/// <summary>
		/// Get the configuration data for the specified cluster.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// </param>
		/// <returns> the cluster configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.ClusterData getCluster(String cluster) throws PulsarAdminException;
		ClusterData GetCluster(string Cluster);

		/// <summary>
		/// Create a new cluster.
		/// <para>
		/// Provisions a new cluster. This operation requires Pulsar super-user privileges.
		/// </para>
		/// <para>
		/// The name cannot contain '/' characters.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="clusterData">
		///            the cluster configuration object
		/// </param>
		/// <exception cref="NotAuthorized">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="ConflictException">
		///             Cluster already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws PulsarAdminException;
		void CreateCluster(string Cluster, ClusterData ClusterData);

		/// <summary>
		/// Update the configuration for a cluster.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="clusterData">
		///            the cluster configuration object
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws PulsarAdminException;
		void UpdateCluster(string Cluster, ClusterData ClusterData);

		/// <summary>
		/// Update peer cluster names.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="peerClusterNames">
		///            list of peer cluster names
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updatePeerClusterNames(String cluster, java.util.LinkedHashSet<String> peerClusterNames) throws PulsarAdminException;
		void UpdatePeerClusterNames(string Cluster, LinkedHashSet<string> PeerClusterNames);

		/// <summary>
		/// Get peer-cluster names
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Domain doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<String> getPeerClusterNames(String cluster) throws PulsarAdminException;
		ISet<string> GetPeerClusterNames(string Cluster);


		/// <summary>
		/// Delete an existing cluster
		/// <para>
		/// Delete a cluster
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Cluster does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteCluster(String cluster) throws PulsarAdminException;
		void DeleteCluster(string Cluster);

		/// <summary>
		/// Get the namespace isolation policies of a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Policies don't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceIsolationData> getNamespaceIsolationPolicies(String cluster) throws PulsarAdminException;
		IDictionary<string, NamespaceIsolationData> GetNamespaceIsolationPolicies(string Cluster);


		/// <summary>
		/// Create a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws PulsarAdminException;
		void CreateNamespaceIsolationPolicy(string Cluster, string PolicyName, NamespaceIsolationData NamespaceIsolationData);


		/// <summary>
		/// Returns list of active brokers with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(String cluster) throws PulsarAdminException;
		IList<BrokerNamespaceIsolationData> GetBrokersWithNamespaceIsolationPolicy(string Cluster);

		/// <summary>
		/// Returns active broker with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="broker">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(String cluster, String broker) throws PulsarAdminException;
		BrokerNamespaceIsolationData GetBrokerWithNamespaceIsolationPolicy(string Cluster, string Broker);


		/// <summary>
		/// Update a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws PulsarAdminException;
		void UpdateNamespaceIsolationPolicy(string Cluster, string PolicyName, NamespaceIsolationData NamespaceIsolationData);


		/// <summary>
		/// Delete a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteNamespaceIsolationPolicy(String cluster, String policyName) throws PulsarAdminException;
		void DeleteNamespaceIsolationPolicy(string Cluster, string PolicyName);

		/// <summary>
		/// Get a single namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Policy doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.NamespaceIsolationData getNamespaceIsolationPolicy(String cluster, String policyName) throws PulsarAdminException;
		NamespaceIsolationData GetNamespaceIsolationPolicy(string Cluster, string PolicyName);

		/// <summary>
		/// Create a domain into cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="FailureDomain">
		///          Domain configurations
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="ConflictException">
		///             Broker already exist into other domain
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws PulsarAdminException;
		void CreateFailureDomain(string Cluster, string DomainName, FailureDomain Domain);


		/// <summary>
		/// Update a domain into cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="FailureDomain">
		///          Domain configurations
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="ConflictException">
		///             Broker already exist into other domain
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws PulsarAdminException;
		void UpdateFailureDomain(string Cluster, string DomainName, FailureDomain Domain);


		/// <summary>
		/// Delete a domain in cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          Domain name
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteFailureDomain(String cluster, String domainName) throws PulsarAdminException;
		void DeleteFailureDomain(string Cluster, string DomainName);

		/// <summary>
		/// Get all registered domains in cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster don't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.FailureDomain> getFailureDomains(String cluster) throws PulsarAdminException;
		IDictionary<string, FailureDomain> GetFailureDomains(string Cluster);

		/// <summary>
		/// Get the domain registered into a cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Domain doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FailureDomain getFailureDomain(String cluster, String domainName) throws PulsarAdminException;
		FailureDomain GetFailureDomain(string Cluster, string DomainName);

	}

}