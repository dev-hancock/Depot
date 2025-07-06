# Depot Auth API

## Users

### Register a new user

```http
POST /auth/users
Content-Type: application/json
```

Creates a new user and a personal tenant.

**Request body:**

```json
{
  "username": "john.doe",
  "password": "P@ssw0rd!",
  "roles": ["user"]
}
```

---

### Authenticate (Login)

```http
POST /auth/session
Content-Type: application/json
```

Starts a new session and returns access/refresh tokens.

**Request body:**

```json
{
  "username": "john.doe",
  "password": "P@ssw0rd!"
}
```

---

### Refresh token

```http
POST /auth/session/refresh
Content-Type: application/json
```

**Request body:**

```json
{
  "refreshToken": "<token>"
}
```

---

### Revoke session (Logout)

```http
DELETE /auth/session
Authorization: Bearer <access_token>
```

---

### Get current user

```http
GET /auth/user
Authorization: Bearer <access_token>
```

---

### Change password

```http
PATCH /auth/user/password
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request body:**

```json
{
  "current": "old_password",
  "updated": "new_password"
}
```

---

## Tenants

### List current user’s tenants

```http
GET /auth/user/tenants
Authorization: Bearer <access_token>
```

---

### Switch active tenant

```http
PATCH /auth/user/tenants/active
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request body:**

```json
{
  "tenantId": "uuid"
}
```

---

## Organizations

### List organizations for current user

```http
GET /auth/user/orgs
Authorization: Bearer <access_token>
```

---

### Create an organization

```http
POST /auth/orgs
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "acme-inc"
}
```

---

### Invite user to organization

```http
POST /auth/orgs/{orgId}/invitations
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request body:**

```json
{
  "username": "jane.doe",
  "roles": ["maintainer"]
}
```

---

### Remove user from organization

```http
DELETE /auth/orgs/{orgId}/members/{userId}
Authorization: Bearer <access_token>
```

