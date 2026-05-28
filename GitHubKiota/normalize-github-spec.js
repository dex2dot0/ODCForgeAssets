const fs = require("fs");
const path = "GitHubKiota/Specs/github-oas-31.json";
const spec = JSON.parse(fs.readFileSync(path, "utf8"));
const paths = spec.paths;
const orgDelete = paths["/orgs/{org}/attestations/{attestation_id}"];
const orgGet = paths["/orgs/{org}/attestations/{subject_digest}"];
orgDelete.get = orgGet.get;
delete paths["/orgs/{org}/attestations/{subject_digest}"];
delete paths["/orgs/{org}/attestations/{attestation_id}"];
paths["/orgs/{org}/attestations/{attestation_key}"] = orgDelete;
orgDelete.delete.parameters[1].name = "attestation_key";
orgDelete.get.parameters[4].name = "attestation_key";
const userDelete = paths["/users/{username}/attestations/{attestation_id}"];
const userGet = paths["/users/{username}/attestations/{subject_digest}"];
userDelete.get = userGet.get;
delete paths["/users/{username}/attestations/{subject_digest}"];
delete paths["/users/{username}/attestations/{attestation_id}"];
paths["/users/{username}/attestations/{attestation_key}"] = userDelete;
userDelete.delete.parameters[1].name = "attestation_key";
userDelete.get.parameters[4].name = "attestation_key";
spec.paths["/repos/{owner}/{repo}/contents/{path}"].get.responses["200"].content["application/json"].schema = { "$ref": "#/components/schemas/content-tree" };
const isNullSchema = (node) => !!node && ((Array.isArray(node.type) && node.type.includes("null")) || node.type === "null");
const firstNonNullType = (types) => types.find((t) => t !== "null") || types[0];
const isArrayLike = (node) => !!node && (node.type === "array" || (Array.isArray(node.type) && node.type.includes("array")));
const isObjectLike = (node) => !!node && (node.type === "object" || (Array.isArray(node.type) && node.type.includes("object")) || node.properties || node.additionalProperties || node.$ref);
const clone = (obj) => JSON.parse(JSON.stringify(obj));
function rewrite(node) {
  if (!node || typeof node !== "object") return;
  if (Array.isArray(node)) {
    for (const item of node) rewrite(item);
    return;
  }
  if (Array.isArray(node.type)) node.type = firstNonNullType(node.type);
  if (node.oneOf || node.anyOf) {
    const key = node.oneOf ? "oneOf" : "anyOf";
    const candidates = node[key].map((c) => clone(c));
    const nonNull = candidates.filter((c) => !isNullSchema(c));
    let replacement;
    if (nonNull.length === 1) {
      replacement = nonNull[0];
    } else if (nonNull.length > 0 && nonNull.every(isArrayLike)) {
      replacement = { type: "array", items: nonNull[0].items ? clone(nonNull[0].items) : { type: "object", additionalProperties: true } };
    } else if (nonNull.length > 0 && nonNull.every(isObjectLike)) {
      replacement = { type: "object", additionalProperties: true };
    } else if (nonNull.length > 0) {
      replacement = nonNull[0];
    } else {
      replacement = { type: "object", additionalProperties: true };
    }
    if (node.description && !replacement.description) replacement.description = node.description;
    for (const existingKey of Object.keys(node)) delete node[existingKey];
    Object.assign(node, replacement);
  }
  if (node.allOf && Array.isArray(node.allOf)) {
    const candidates = node.allOf.map((c) => clone(c));
    if (candidates.every(isObjectLike)) {
      const merged = { type: "object", properties: {}, required: [] };
      for (const candidate of candidates) {
        if (candidate.$ref) merged.additionalProperties = true;
        if (candidate.properties) Object.assign(merged.properties, candidate.properties);
        if (Array.isArray(candidate.required)) merged.required.push(...candidate.required);
        if (candidate.additionalProperties !== undefined) merged.additionalProperties = candidate.additionalProperties;
        if (candidate.description && !merged.description) merged.description = candidate.description;
      }
      if (node.description && !merged.description) merged.description = node.description;
      merged.required = [...new Set(merged.required)];
      for (const existingKey of Object.keys(node)) delete node[existingKey];
      Object.assign(node, merged);
    }
  }
  delete node.discriminator;
  for (const value of Object.values(node)) rewrite(value);
}
rewrite(spec);
fs.writeFileSync(path, JSON.stringify(spec, null, 2));
