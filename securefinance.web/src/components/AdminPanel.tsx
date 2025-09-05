import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Alert,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Stack
} from '@mui/material';
import { apiService } from '../services/apiService';

interface UsageStats {
  totalRequests: number;
  cachedRequests: number;
  averageResponseTime: number;
  errorCount: number;
  requestsByEndpoint: Record<string, number>;
  periodStart: string;
  periodEnd: string;
}

interface SystemHealth {
  status: string;
  database: string;
  timestamp: string;
  dataSources: Array<{
    name: string;
    isActive: boolean;
    lastHealthCheck: string;
  }>;
}

const AdminPanel: React.FC = () => {
  const [stats, setStats] = useState<UsageStats | null>(null);
  const [health, setHealth] = useState<SystemHealth | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Mock data for demo
        setTimeout(() => {
          setStats({
            totalRequests: 1547,
            cachedRequests: 1241,
            averageResponseTime: 156,
            errorCount: 3,
            requestsByEndpoint: {
              '/api/stock/*': 847,
              '/api/crypto/*': 423,
              '/api/rates/*': 277
            },
            periodStart: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
            periodEnd: new Date().toISOString()
          });
          
          setHealth({
            status: 'Healthy',
            database: 'Connected',
            timestamp: new Date().toISOString(),
            dataSources: [
              { name: 'Alpha Vantage', isActive: true, lastHealthCheck: new Date().toISOString() },
              { name: 'CoinGecko', isActive: true, lastHealthCheck: new Date().toISOString() },
              { name: 'FRED', isActive: true, lastHealthCheck: new Date().toISOString() }
            ]
          });
          
          setLoading(false);
        }, 1000);
        
      } catch (err: any) {
        setError('Failed to fetch admin data');
        console.error('Admin panel error:', err);
        setLoading(false);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 30000);
    
    return () => clearInterval(interval);
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom sx={{ mb: 4, fontWeight: 'bold' }}>
        🛠️ Admin Panel
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Stack spacing={3} direction={{ xs: 'column', md: 'row' }} sx={{ mb: 4 }}>
        {/* System Health */}
        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              System Health
            </Typography>
            {health && (
              <Box>
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                  <Typography>Overall Status:</Typography>
                  <Chip 
                    label={health.status} 
                    color={health.status === 'Healthy' ? 'success' : 'error'}
                  />
                </Box>
                <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                  <Typography>Database:</Typography>
                  <Chip 
                    label={health.database} 
                    color={health.database === 'Connected' ? 'success' : 'error'}
                    size="small"
                  />
                </Box>
                <Typography variant="body2" color="textSecondary">
                  Last Check: {new Date(health.timestamp).toLocaleString()}
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>

        {/* Usage Statistics */}
        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Usage Statistics
            </Typography>
            {stats && (
              <Box>
                <Typography variant="body2">
                  Total Requests: <strong>{stats.totalRequests.toLocaleString()}</strong>
                </Typography>
                <Typography variant="body2">
                  Cached Requests: <strong>{stats.cachedRequests.toLocaleString()}</strong>
                </Typography>
                <Typography variant="body2">
                  Cache Hit Rate: <strong>{((stats.cachedRequests / stats.totalRequests) * 100).toFixed(1)}%</strong>
                </Typography>
                <Typography variant="body2">
                  Avg Response Time: <strong>{stats.averageResponseTime.toFixed(0)}ms</strong>
                </Typography>
                <Typography variant="body2">
                  Error Rate: <strong>{((stats.errorCount / stats.totalRequests) * 100).toFixed(2)}%</strong>
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Stack>

      <Stack spacing={3} direction={{ xs: 'column', md: 'row' }}>
        {/* Data Sources */}
        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Data Sources
            </Typography>
            {health?.dataSources && (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Source</TableCell>
                      <TableCell>Status</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {health.dataSources.map((source, index) => (
                      <TableRow key={index}>
                        <TableCell>{source.name}</TableCell>
                        <TableCell>
                          <Chip 
                            label={source.isActive ? 'Active' : 'Inactive'} 
                            color={source.isActive ? 'success' : 'error'}
                            size="small"
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>

        {/* Endpoint Usage */}
        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Endpoint Usage
            </Typography>
            {stats?.requestsByEndpoint && (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Endpoint</TableCell>
                      <TableCell align="right">Requests</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {Object.entries(stats.requestsByEndpoint).map(([endpoint, count]) => (
                      <TableRow key={endpoint}>
                        <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                          {endpoint}
                        </TableCell>
                        <TableCell align="right">
                          <strong>{count}</strong>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
};

export default AdminPanel;