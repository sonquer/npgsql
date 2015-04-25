// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational;

namespace Npgsql.EntityFramework7.Query
{
    public class NpgsqlValueReaderFactoryFactory : INpgsqlValueReaderFactoryFactory
    {
        public virtual IRelationalValueReaderFactory CreateValueReaderFactory() => new NpgsqlValueReaderFactory();
    }
}
