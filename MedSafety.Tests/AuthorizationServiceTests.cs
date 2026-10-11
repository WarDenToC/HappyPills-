using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MedSafety.Tests
{
    [TestClass]
    public class AuthorizationServiceTests
    {
        private static readonly Type AuthorizationServiceType = FindType("MedSafety.Core.Security.AuthorizationService");
        private static readonly Type AuditLogInterfaceType = FindType("MedSafety.Core.Audit.IMAuditLog");
        private static readonly Type PermissionType = FindType("MedSafety.Core.Security.Permission") ?? FindType("MedSafety.Core.Security.PermissionType") ?? FindType("MedSafety.Core.Permission");

        [DataTestMethod]
        [DataRow("Prescriber", "CreatePrescription")]
        [DataRow("Pharmacist", "ReviewPrescription")]
        [DataRow("Admin", "ViewDashboard")]
        public void Authorize_WhenRoleHasMatchingPermission_ReturnsTrue_AndWritesNothingToAuditLog(string role, string permissionName)
        {
            var actor = "actor-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var permission = ResolvePermission(permissionName);
            var auditLog = CreateAuditLog();
            var service = CreateAuthorizationService(auditLog);

            var result = InvokeAuthorize(service, actor, role, permission);

            Assert.IsTrue(result, $"Expected {role} to be allowed to {permissionName}.");
            Assert.AreEqual(0, GetAuditEntries(auditLog).Count, "Allowed access should not write to the audit log.");
        }

        [DataTestMethod]
        [DataRow("Prescriber", "ReviewPrescription")]
        [DataRow("Prescriber", "ViewDashboard")]
        [DataRow("Pharmacist", "CreatePrescription")]
        [DataRow("Pharmacist", "ViewDashboard")]
        [DataRow("Admin", "CreatePrescription")]
        [DataRow("Admin", "ReviewPrescription")]
        [DataRow("Doctor", "CreatePrescription")]
        [DataRow("Doctor", "ReviewPrescription")]
        [DataRow("Doctor", "ViewDashboard")]
        public void Authorize_WhenRoleDoesNotMatchPermission_ReturnsFalse_AndWritesSingleAccessDeniedAuditEntry(string role, string permissionName)
        {
            var actor = "actor-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var permission = ResolvePermission(permissionName);
            var auditLog = CreateAuditLog();
            var service = CreateAuthorizationService(auditLog);

            var result = InvokeAuthorize(service, actor, role, permission);

            Assert.IsFalse(result, $"Expected {role} to be denied the {permissionName} permission.");

            var entries = GetAuditEntries(auditLog);
            Assert.AreEqual(1, entries.Count, "Denied access should write exactly one audit entry.");

            var entry = entries[0];
            var entryActor = GetFirstMatchingValue(entry, "Actor", "ActorName", "UserName", "User");
            var entryRole = GetFirstMatchingValue(entry, "Role", "UserRole");
            var action = GetFirstMatchingValue(entry, "Action", "Operation");
            var detail = GetFirstMatchingValue(entry, "Detail", "Message", "Description");

            Assert.IsTrue(
                string.Equals(Convert.ToString(entryActor), actor, StringComparison.Ordinal) ||
                string.Equals(Convert.ToString(entryRole), role, StringComparison.Ordinal),
                "Denied audit entry should include the actor or the role used in the authorization check.");

            Assert.AreEqual(role, Convert.ToString(entryRole), "Audit entry role should match the role being resolved.");
            Assert.AreEqual("Access denied", Convert.ToString(action), "Audit entry action should be 'Access denied'.");
            StringAssert.Contains(Convert.ToString(detail), permissionName, "Audit entry detail should identify the requested permission.");
        }

        private static Type FindType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName.Split('.').Last());
        }

        private static object ResolvePermission(string permissionName)
        {
            if (PermissionType == null)
            {
                throw new AssertFailedException("Permission type was not found. Expected a permission enum or class in MedSafety.Core.Security.");
            }

            var field = PermissionType.GetFields(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(f => string.Equals(f.Name, permissionName, StringComparison.Ordinal) ||
                    string.Equals(f.Name, permissionName.Replace(".", string.Empty), StringComparison.Ordinal));

            if (field != null)
            {
                return field.GetValue(null);
            }

            var property = PermissionType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(p => string.Equals(p.Name, permissionName, StringComparison.Ordinal));

            if (property != null)
            {
                return property.GetValue(null);
            }

            var enumValue = Enum.Parse(PermissionType, permissionName, true);
            return enumValue;
        }

        private static object CreateAuditLog()
        {
            if (AuditLogInterfaceType == null)
            {
                throw new AssertFailedException("IMAuditLog type was not found. Expected the MedSafety.Core.Audit.IMAuditLog interface.");
            }

            var concreteType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => AuditLogInterfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (concreteType == null)
            {
                throw new AssertFailedException("No concrete implementation of IMAuditLog could be found. Use the real audit log implementation, not a mock.");
            }

            return Activator.CreateInstance(concreteType);
        }

        private static object CreateAuthorizationService(object auditLog)
        {
            if (AuthorizationServiceType == null)
            {
                throw new AssertFailedException("AuthorizationService type was not found. Expected MedSafety.Core.Security.AuthorizationService.");
            }

            var constructor = AuthorizationServiceType
                .GetConstructors()
                .FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == AuditLogInterfaceType);

            if (constructor != null)
            {
                return constructor.Invoke(new[] { auditLog });
            }

            var parameterlessCtor = AuthorizationServiceType.GetConstructor(Type.EmptyTypes);
            if (parameterlessCtor != null)
            {
                var service = parameterlessCtor.Invoke(Array.Empty<object>());

                var property = AuthorizationServiceType.GetProperty("AuditLog", BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite && property.PropertyType == AuditLogInterfaceType)
                {
                    property.SetValue(service, auditLog);
                }

                var field = AuthorizationServiceType.GetField("_auditLog", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? AuthorizationServiceType.GetField("auditLog", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? AuthorizationServiceType.GetField("_auditLogger", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && field.FieldType == AuditLogInterfaceType)
                {
                    field.SetValue(service, auditLog);
                }

                return service;
            }

            throw new AssertFailedException("AuthorizationService could not be constructed with the real IMAuditLog implementation.");
        }

        private static bool InvokeAuthorize(object service, string actor, string role, object permission)
        {
            var method = service.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Authorize" && m.ReturnType == typeof(bool));

            if (method == null)
            {
                throw new AssertFailedException("Authorize method was not found on AuthorizationService.");
            }

            var parameters = method.GetParameters();
            var values = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterType = parameter.ParameterType;
                var parameterName = parameter.Name ?? string.Empty;

                if (parameterType == typeof(string) && (parameterName.Contains("actor", StringComparison.OrdinalIgnoreCase) || parameterName.Contains("user", StringComparison.OrdinalIgnoreCase)))
                {
                    values[i] = actor;
                    continue;
                }

                if (parameterType == typeof(string) && (parameterName.Contains("role", StringComparison.OrdinalIgnoreCase)))
                {
                    values[i] = role;
                    continue;
                }

                if (parameterType == typeof(string) && (parameterName.Contains("permission", StringComparison.OrdinalIgnoreCase) || parameterName.Contains("action", StringComparison.OrdinalIgnoreCase)))
                {
                    values[i] = permission?.ToString();
                    continue;
                }

                if (parameterType == PermissionType)
                {
                    values[i] = permission;
                    continue;
                }

                if (parameterType == typeof(string) || parameterType == typeof(object))
                {
                    values[i] = role;
                    continue;
                }

                values[i] = permission;
            }

            return (bool)method.Invoke(service, values);
        }

        private static IList<object> GetAuditEntries(object auditLog)
        {
            if (auditLog == null)
            {
                return Array.Empty<object>();
            }

            var entriesProperty = auditLog.GetType().GetProperty("Entries", BindingFlags.Public | BindingFlags.Instance)
                ?? auditLog.GetType().GetProperty("AuditEntries", BindingFlags.Public | BindingFlags.Instance)
                ?? auditLog.GetType().GetProperty("Items", BindingFlags.Public | BindingFlags.Instance)
                ?? auditLog.GetType().GetProperty("LogEntries", BindingFlags.Public | BindingFlags.Instance);

            if (entriesProperty != null)
            {
                var value = entriesProperty.GetValue(auditLog);
                if (value is IEnumerable<object> items)
                {
                    return items.ToList();
                }

                if (value is System.Collections.IEnumerable enumerable)
                {
                    return enumerable.Cast<object>().ToList();
                }
            }

            var entriesMethod = auditLog.GetType().GetMethod("GetEntries", BindingFlags.Public | BindingFlags.Instance)
                ?? auditLog.GetType().GetMethod("ReadEntries", BindingFlags.Public | BindingFlags.Instance)
                ?? auditLog.GetType().GetMethod("GetAuditEntries", BindingFlags.Public | BindingFlags.Instance);

            if (entriesMethod != null)
            {
                var value = entriesMethod.Invoke(auditLog, Array.Empty<object>());
                if (value is IEnumerable<object> items2)
                {
                    return items2.ToList();
                }

                if (value is System.Collections.IEnumerable enumerable2)
                {
                    return enumerable2.Cast<object>().ToList();
                }
            }

            return Array.Empty<object>();
        }

        private static object GetFirstMatchingValue(object entry, params string[] candidatePropertyNames)
        {
            if (entry == null)
            {
                return null;
            }

            foreach (var name in candidatePropertyNames)
            {
                var property = entry.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    return property.GetValue(entry);
                }

                var field = entry.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance)
                    ?? entry.GetType().GetField("_" + name, BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? entry.GetType().GetField(name.ToLowerInvariant(), BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    return field.GetValue(entry);
                }
            }

            return null;
        }
    }
}
