# Endpoints

All endpoints require:

* `Authorization: Bearer <access_token>` header
* Tenant context is derived from JWT claims (e.g., `tenant_id`, `sub`, etc.)

---

## Artifact Endpoints

### Upload Artifact

```http
POST /api/artifacts/{repository}/{*path}
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

**Description**: Upload one or more files to a repository under the specified path.

---

### Download Artifact

```http
GET /api/artifacts/{repository}/{*path}
Authorization: Bearer <access_token>
```

**Description**: Download a specific artifact file.

---

### Delete Artifact or Folder

```http
DELETE /api/artifacts/{repository}/{*path}
Authorization: Bearer <access_token>
```

**Description**: Delete a specific file or recursively delete a folder.

---

### Get Artifact Metadata

```http
GET /api/meta/{repository}/{*path}
Authorization: Bearer <access_token>
Accept: application/json
```

**Description**: Retrieve metadata (e.g., name, size, type, hash) for the specified artifact.

---

## Repository Endpoints

### List Repositories

```http
GET /api/repos
Authorization: Bearer <access_token>
Accept: application/json
```

**Description**: List all repositories owned by the current tenant.

---

### Create Repository

```http
POST /api/repos
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Body Example**:

```json
{
  "name": "my-repo",
  "type": "generic"
}
```

**Description**: Create a new repository for the tenant.

---

### Delete Repository

```http
DELETE /api/repos/{repository}
Authorization: Bearer <access_token>
```

**Description**: Delete the entire repository and its contents.

---

