//
// public static class SessionValidator
// {
//     public static void Assert() {  
//         var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
//
//         Assert.NotNull(result);
//         Assert.NotNull(result.AccessToken);
//         Assert.NotNull(result.RefreshToken);
//
//         var token = _handler.ReadJwtToken(result.AccessToken);
//
//         var sessionId = token.Claims.SingleOrDefault(x => x.Type == "jti");
//         var userId = token.Claims.SingleOrDefault(x => x.Type == "sub");
//
//         Assert.NotNull(sessionId);
//         Assert.NotNull(userId);
//
//         Assert.Equal(session.Id.Value.ToString(), sessionId.Value);
//         Assert.Equal(session.UserId.Value.ToString(), userId.Value);
//
//         var exists = await _cache.GetAsync(session.Id.Value.ToString());
//
//         Assert.NotNull(exists);
//         Assert.Equal([0x1], exists);}
// }

