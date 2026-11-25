#!/bin/sh
set -eu
# Hash passwords at start so only Aspire secrets hold them, never git
mosquitto_passwd -c -b /tmp/passwd sensor "$MQTT_SENSOR_PASSWORD"
mosquitto_passwd -b /tmp/passwd middleware "$MQTT_MIDDLEWARE_PASSWORD"
# Mosquitto warns on files it doesn't own, and the mount is read-only
cp /mosquitto/config/acl /tmp/acl
chown mosquitto:mosquitto /tmp/passwd /tmp/acl
chmod 0600 /tmp/passwd /tmp/acl
exec mosquitto -c /mosquitto/config/mosquitto.conf
