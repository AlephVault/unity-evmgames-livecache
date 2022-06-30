using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlephVault.Unity.EVMGames.Nethereum.Hex.HexTypes;
using AlephVault.Unity.RemoteStorage.StandardHttp.Types;
using AlephVault.Unity.RemoteStorage.Types.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace AlephVault.Unity.EVMGames.LiveCache
{
    namespace Types
    {
        namespace Cache
        {
            /// <summary>
            ///   An abstraction over an EVM ERC721 ownership
            ///   resource. Used to query ownerships and to
            ///   reset the cache.
            /// </summary>
            public class ERC721OwnershipCacheHandler
            {
                private class CollectionOfResultEntry
                {
                    [JsonProperty("token")]
                    public string Token;
                }

                private class CollectionsResultEntry
                {
                    [JsonProperty("owner")]
                    public string Owner;
                    
                    [JsonProperty("token")]
                    public string Token;
                }

                /// <summary>
                ///   The related resource.
                /// </summary>
                public readonly SimpleResource<ERC721Ownership> ERC721OwnershipResource;
                
                /// <summary>
                ///   Creates the instance from a specific resource
                ///   (a root one) and a resource.
                /// </summary>
                /// <param name="root">The root resource</param>
                /// <param name="resource">The resource key</param>
                public ERC721OwnershipCacheHandler(Root root, string resource = "evm-erc721-ownership")
                {
                    ERC721OwnershipResource = (SimpleResource<ERC721Ownership>)root.GetSimple<ERC721Ownership>(
                        resource
                    );
                }
                
                /// <summary>
                ///   Resets the cache for a given contract.
                /// </summary>
                /// <param name="contractKey">The contract key</param>
                public async Task<Result<JObject, string>> Reset(string contractKey)
                {
                    return await ERC721OwnershipResource.OperationToJson(
                        "reset-cache", new Dictionary<string, string>()
                        {
                            { "contract-key", contractKey }
                        }
                    );
                }
                
                // Parses a token.
                private HexBigInteger Parse(string value)
                {
                    try
                    {
                        return new HexBigInteger(value);
                    }
                    catch
                    {
                        return null;
                    }
                }

                /// <summary>
                ///   Queries the ownerships inside a contract.
                /// </summary>
                /// <param name="contractKey">The contract key</param>
                /// <param name="offset">The offset for the query</param>
                /// <param name="limit">The limit for the query</param>
                /// <returns>all the ownerships</returns>
                public async Task<Result<Tuple<string, HexBigInteger>[], string>> Collections(
                    string contractKey, int offset, int limit
                ) {
                    Result<CollectionsResultEntry[], string> result = await ERC721OwnershipResource.ViewTo<CollectionsResultEntry[]>(
                        "collections", new Dictionary<string, string>
                        {
                            { "contract-key", contractKey },
                            { "offset", offset.ToString() },
                            { "limit", limit.ToString() }
                        }
                    );

                    if (result.Code == ResultCode.Ok)
                    {
                        return new Result<Tuple<string, HexBigInteger>[], string>
                        {
                            Element = (from element in result.Element
                                       select new Tuple<string, HexBigInteger>(
                                           element.Owner, Parse(element.Token)
                                       )).ToArray(),
                            Code = result.Code
                        };
                    }
                    
                    return new Result<Tuple<string, HexBigInteger>[], string>
                    {
                        Element = null,
                        Code = result.Code
                    };
                }

                /// <summary>
                ///   Queries the ownerships of an address inside a contract.
                /// </summary>
                /// <param name="contractKey">The contract key</param>
                /// <param name="owner">The address of the owner</param>
                /// <param name="offset">The offset for the query</param>
                /// <param name="limit">The limit for the query</param>
                /// <returns>the list of ownerships for the address</returns>
                public async Task<Result<HexBigInteger[], string>> CollectionOf(
                    string contractKey, string owner, uint offset, uint limit
                ) {
                    Result<CollectionOfResultEntry[], string> result = 
                        await ERC721OwnershipResource.ViewTo<CollectionOfResultEntry[]>(
                        "collection-of", new Dictionary<string, string>
                        {
                            { "contract-key", contractKey },
                            { "owner", owner },
                            { "offset", offset.ToString() },
                            { "limit", limit.ToString() },
                        }
                    );

                    if (result.Code == ResultCode.Ok)
                    {
                        return new Result<HexBigInteger[], string>
                        {
                            Element = (from element in result.Element
                                       select Parse(element.Token)).ToArray(),
                            Code = result.Code
                        };
                    }

                    return new Result<HexBigInteger[], string>
                    {
                        Element = null,
                        Code = result.Code
                    };
                }
            }
        }
    }
}
