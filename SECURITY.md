# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of Task Scheduler Service seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### Please Do NOT:

- Open a public GitHub issue for security vulnerabilities
- Disclose the vulnerability publicly before it has been addressed

### Please DO:

1. **Report privately** using GitHub Security Advisories:
   - Go to the repository's Security tab
   - Click "Report a vulnerability"
   - Fill in the details

2. **Or email us** at: [security@yourdomain.com]

### What to Include:

- **Description**: A clear description of the vulnerability
- **Impact**: The potential impact of the vulnerability
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Affected Versions**: Which versions are affected
- **Suggested Fix**: If you have ideas for fixing it
- **POC**: Proof of concept code (if applicable)

### Response Timeline:

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: Depends on severity
  - Critical: Within 7 days
  - High: Within 14 days
  - Medium: Within 30 days
  - Low: Next release cycle

### Security Update Process:

1. We will acknowledge receipt of your report
2. We will investigate and validate the vulnerability
3. We will develop a fix
4. We will test the fix
5. We will release a security update
6. We will publicly disclose the vulnerability (with credit to reporter, if desired)

## Security Best Practices

When deploying Task Scheduler Service, follow these security best practices:

### Service Configuration

1. **Run with Least Privilege**
   - Use a dedicated service account
   - Grant only necessary permissions
   - Never run as Administrator unless required

2. **Secure Configuration Files**
   - Restrict file permissions on `appsettings.json`
   - Use Windows DPAPI for sensitive data
   - Consider Azure Key Vault for production

3. **Network Security**
   - Use TLS/SSL for SMTP connections
   - Restrict network access as needed
   - Use firewall rules appropriately

### Job Security

1. **Script Validation**
   - Sign PowerShell scripts
   - Validate script sources
   - Use execution policies

2. **Input Validation**
   - Validate all job parameters
   - Sanitize file paths
   - Avoid dynamic script generation

3. **Access Control**
   - Limit job file access
   - Use read-only permissions where possible
   - Audit job execution

### Monitoring and Auditing

1. **Enable Logging**
   - Monitor all job executions
   - Review logs regularly
   - Alert on suspicious activity

2. **Email Security**
   - Use app passwords, not main passwords
   - Enable 2FA on email accounts
   - Monitor email notifications

## Known Security Considerations

### Password Storage

The current implementation stores SMTP passwords in `appsettings.json`. For production:

1. Use environment variables
2. Use Windows Credential Manager
3. Use Azure Key Vault
4. Use encrypted configuration sections

### Script Execution

PowerShell scripts run with the service account's permissions:

1. Use script signing
2. Apply execution policies
3. Validate script content
4. Use least privilege

### File System Access

Jobs can access files based on service account permissions:

1. Use dedicated service account
2. Restrict file access
3. Audit file operations
4. Use read-only where possible

## Vulnerability Disclosure Policy

We follow responsible disclosure principles:

1. **Private Disclosure**: Report privately first
2. **Investigation Period**: 90 days maximum
3. **Public Disclosure**: After fix is available
4. **Credit**: We will credit researchers (if desired)

## Security Updates

Security updates will be:

- Released as patch versions (e.g., 1.0.1)
- Documented in CHANGELOG.md
- Announced in GitHub Releases
- Tagged with security label

## Contact

For security concerns:
- **GitHub Security Advisories**: Preferred method
- **Email**: [security@yourdomain.com]
- **Response Time**: 48 hours maximum

## Hall of Fame

We thank the following security researchers:

<!-- List will be updated as vulnerabilities are reported and fixed -->

---

**Last Updated**: 2024-01-15  
**Version**: 1.0.0
