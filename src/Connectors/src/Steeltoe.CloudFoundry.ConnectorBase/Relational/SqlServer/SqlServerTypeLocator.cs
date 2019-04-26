﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Steeltoe.CloudFoundry.Connector.Relational.SqlServer
{
    /// <summary>
    /// Assemblies and types used for interacting with Microsoft SQL Server
    /// </summary>
    public static class SqlServerTypeLocator
    {
        /// <summary>
        /// List of supported SQL Server Client assemblies
        /// </summary>
        public static readonly string[] Assemblies = new string[] { "System.Data.SqlClient" };

        /// <summary>
        /// List of SQL Server types that implement IDbConnection
        /// </summary>
        public static readonly string[] ConnectionTypeNames = new string[] { "System.Data.SqlClient.SqlConnection" };

        /// <summary>
        /// Gets SqlConnection from a SQL Server Library
        /// </summary>
        /// <exception cref="ConnectorException">When type is not found</exception>
        public static Type SqlConnection
        {
            get
            {
                var type = ConnectorHelpers.FindType(Assemblies, ConnectionTypeNames);
                if (type == null)
                {
                    throw new ConnectorException("Unable to find SqlConnection, are you missing a Microsoft SQL Server ADO.NET assembly?");
                }

                return type;
            }
        }
    }
}
