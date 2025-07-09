import React, { useEffect, useState } from 'react';
import { Upload, Button, Table, Alert, Form, Input, InputNumber, message, Card, Row, Col, Space, Typography, Badge } from 'antd';
import { UploadOutlined, WarningOutlined, SyncOutlined, SettingOutlined } from '@ant-design/icons';
import axios from 'axios';

const { Title, Text } = Typography;

const App = () => {
  const [latestData, setLatestData] = useState([]);
  const [alerts, setAlerts] = useState([]);
  const [fileUploading, setFileUploading] = useState(false);
  const [form] = Form.useForm();

  const fetchLatestData = async () => {
    try {
      const res = await axios.get('http://localhost:5070/api/latest');
      if (res.data.success) {
        setLatestData(res.data.data);
      }
    } catch (error) {
      console.error('Lỗi tải dữ liệu mới nhất:', error);
    }
  };

  const fetchAlerts = async () => {
    try {
      const res = await axios.get('http://localhost:5070/api/status/alert');
      if (res.data.success && res.data.hasAlerts) {
        setAlerts(res.data.alerts);
      } else {
        setAlerts([]);
      }
    } catch (error) {
      console.error('Lỗi lấy cảnh báo:', error);
    }
  };

  useEffect(() => {
    fetchLatestData();
    fetchAlerts();
    const interval = setInterval(() => {
      fetchLatestData();
      fetchAlerts();
    }, 10000); // mỗi 10s

    return () => clearInterval(interval);
  }, []);

  const handleUpload = async (file) => {
    const formData = new FormData();
    formData.append('file', file);

    try {
      setFileUploading(true);
      const res = await axios.post('http://localhost:5070/api/upload-csv', formData);
      message.success(res.data.message || 'Tải file thành công');
      fetchLatestData();
      fetchAlerts();
    } catch (err) {
      message.error(err.response?.data?.message || 'Lỗi khi tải file');
    } finally {
      setFileUploading(false);
    }

    return false; // để Upload không tự upload
  };

  const onThresholdSubmit = async (values) => {
    try {
      const res = await axios.post('http://localhost:5070/api/set-threshold', values);
      message.success(res.data.message || 'Cập nhật ngưỡng thành công');
    } catch (err) {
      message.error(err.response?.data?.message || 'Lỗi khi cập nhật ngưỡng');
    }
  };

  const columns = [
    { 
      title: 'Thời gian', 
      dataIndex: 'timestamp',
      render: (text) => <Text style={{ color: '#6b46c1' }}>{text}</Text>
    },
    { 
      title: 'Sensor', 
      dataIndex: 'sensorId',
      render: (text) => <Badge color="#7c3aed" text={text} />
    },
    { 
      title: 'Giá trị', 
      dataIndex: 'value',
      render: (text) => <Text strong style={{ color: '#059669' }}>{text}</Text>
    },
    { 
      title: 'Đơn vị', 
      dataIndex: 'unit',
      render: (text) => <Text type="secondary">{text}</Text>
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isAlert',
      render: (val) => val ? (
        <Badge status="error" text="Cảnh báo" />
      ) : (
        <Badge status="success" text="Bình thường" />
      )
    }
  ];

  const canvaColors = {
    primary: '#7c3aed',
    secondary: '#a855f7',
    accent: '#ec4899',
    success: '#059669',
    warning: '#d97706',
    error: '#dc2626',
    background: '#faf5ff',
    cardBg: '#ffffff',
    textPrimary: '#1f2937',
    textSecondary: '#6b7280'
  };

  return (
    <div style={{ 
      minHeight: '100vh',
      background: `linear-gradient(135deg, ${canvaColors.background} 0%, #f3e8ff 100%)`,
      padding: '24px'
    }}>
      <div style={{ maxWidth: 1200, margin: '0 auto' }}>
        {/* Header */}
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={1} style={{ 
            color: canvaColors.primary,
            marginBottom: 8,
            fontWeight: 700
          }}>
             Hệ thống Giám sát
          </Title>
          <Text style={{ color: canvaColors.textSecondary, fontSize: 16 }}>
            Theo dõi và quản lý cảm biến thời gian thực
          </Text>
        </div>

        {/* Control Panel */}
        <Row gutter={[24, 24]} style={{ marginBottom: 32 }}>
          <Col xs={24} md={12}>
            <Card 
              title={
                <Space>
                  <UploadOutlined style={{ color: canvaColors.primary }} />
                  <span>Tải dữ liệu</span>
                </Space>
              }
              style={{ 
                borderRadius: 16,
                boxShadow: '0 4px 20px rgba(124, 58, 237, 0.1)',
                border: `1px solid ${canvaColors.secondary}20`
              }}
              headStyle={{ 
                background: `linear-gradient(90deg, ${canvaColors.primary}10, ${canvaColors.secondary}10)`,
                borderRadius: '16px 16px 0 0'
              }}
            >
              <Upload beforeUpload={handleUpload} showUploadList={false}>
                <Button 
                  icon={<UploadOutlined />} 
                  loading={fileUploading}
                  size="large"
                  style={{
                    background: `linear-gradient(135deg, ${canvaColors.primary}, ${canvaColors.secondary})`,
                    border: 'none',
                    color: 'white',
                    borderRadius: 12,
                    height: 48,
                    fontSize: 16,
                    fontWeight: 600
                  }}
                  block
                >
                  Tải lên file CSV
                </Button>
              </Upload>
            </Card>
          </Col>

          <Col xs={24} md={12}>
            <Card 
              title={
                <Space>
                  <SettingOutlined style={{ color: canvaColors.accent }} />
                  <span>Cài đặt ngưỡng</span>
                </Space>
              }
              style={{ 
                borderRadius: 16,
                boxShadow: '0 4px 20px rgba(236, 72, 153, 0.1)',
                border: `1px solid ${canvaColors.accent}20`
              }}
              headStyle={{ 
                background: `linear-gradient(90deg, ${canvaColors.accent}10, ${canvaColors.secondary}10)`,
                borderRadius: '16px 16px 0 0'
              }}
            >
              <Form form={form} layout="vertical" onFinish={onThresholdSubmit}>
                <Form.Item 
                  name="sensorId" 
                  label="Sensor ID"
                  rules={[{ required: true, message: 'Nhập sensor ID' }]}
                >
                  <Input 
                    placeholder="VD: TEMP001" 
                    style={{ borderRadius: 8, height: 40 }}
                  />
                </Form.Item>
                <Form.Item 
                  name="threshold" 
                  label="Ngưỡng cảnh báo"
                  rules={[{ required: true, message: 'Nhập ngưỡng' }]}
                >
                  <InputNumber 
                    placeholder="Giá trị ngưỡng" 
                    style={{ width: '100%', borderRadius: 8, height: 40 }}
                  />
                </Form.Item>
                <Form.Item>
                  <Button 
                    type="primary" 
                    htmlType="submit"
                    size="large"
                    style={{
                      background: `linear-gradient(135deg, ${canvaColors.accent}, ${canvaColors.secondary})`,
                      border: 'none',
                      borderRadius: 8,
                      height: 40,
                      fontWeight: 600
                    }}
                    block
                  >
                    Cập nhật ngưỡng
                  </Button>
                </Form.Item>
              </Form>
            </Card>
          </Col>
        </Row>

        {/* Alerts */}
        {alerts.length > 0 && (
          <Card 
            style={{ 
              marginBottom: 32,
              borderRadius: 16,
              border: `2px solid ${canvaColors.error}30`,
              background: `linear-gradient(135deg, ${canvaColors.error}05, #fff)`
            }}
          >
            <Alert
              type="error"
              showIcon
              icon={<WarningOutlined style={{ fontSize: 20 }} />}
              message={
                <Title level={4} style={{ color: canvaColors.error, margin: 0 }}>
                   Cảnh báo vượt ngưỡng ({alerts.length})
                </Title>
              }
              description={
                <div style={{ marginTop: 12 }}>
                  {alerts.map((a) => (
                    <div key={a.id} style={{
                      background: 'white',
                      padding: '12px 16px',
                      borderRadius: 8,
                      marginBottom: 8,
                      border: `1px solid ${canvaColors.error}20`,
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center'
                    }}>
                      <Space>
                        <Badge color={canvaColors.error} />
                        <Text strong>{a.sensorId}</Text>
                        <Text>Giá trị: {a.value} {a.unit}</Text>
                      </Space>
                      <Text type="secondary">{a.timestamp}</Text>
                    </div>
                  ))}
                </div>
              }
              style={{ 
                border: 'none',
                background: 'transparent'
              }}
            />
          </Card>
        )}

        {/* Data Table */}
        <Card 
          title={
            <Space>
              <SyncOutlined style={{ color: canvaColors.success }} />
              <span>Dữ liệu cảm biến mới nhất</span>
              <Badge count={latestData.length} style={{ backgroundColor: canvaColors.success }} />
            </Space>
          }
          style={{ 
            borderRadius: 16,
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
            border: 'none'
          }}
          headStyle={{ 
            background: `linear-gradient(90deg, ${canvaColors.success}10, ${canvaColors.primary}10)`,
            borderRadius: '16px 16px 0 0'
          }}
        >
          <Table
            rowKey="id"
            dataSource={latestData}
            columns={columns}
            pagination={{
              pageSize: 10,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total) => `Tổng ${total} bản ghi`
            }}
            style={{ 
              background: 'white',
              borderRadius: 12
            }}
            rowClassName={(record) => 
              record.isAlert ? 'alert-row' : 'normal-row'
            }
          />
        </Card>
      </div>

      <style jsx global>{`
        .alert-row {
          background: ${canvaColors.error}08 !important;
        }
        .normal-row:hover {
          background: ${canvaColors.primary}05 !important;
        }
        .ant-table-thead > tr > th {
          background: ${canvaColors.primary}10 !important;
          color: ${canvaColors.primary} !important;
          font-weight: 600 !important;
          border: none !important;
        }
        .ant-table-tbody > tr > td {
          border-bottom: 1px solid ${canvaColors.primary}10 !important;
        }
      `}</style>
    </div>
  );
};

export default App;