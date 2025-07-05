#!/bin/bash
# scripts/gen-keys.sh

set -e

mkdir -p keys

echo "Generating JWT ECDSA key pair..."

openssl ecparam -name prime256v1 -genkey -noout -out keys/jwt_private.pem
openssl ec -in keys/jwt_private.pem -pubout -out keys/jwt_public.pem

chmod 644 keys/*.pem

echo "Keys generated at ./keys"
