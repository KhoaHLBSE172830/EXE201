import apiClient from '../api/apiClient';
import { API_ENDPOINTS } from '../api/endpoints';
import { mockSuccess, mockError } from '../mocks/mockHelper';
import { mockBusinessData } from '../mocks/mockBusiness';

const STORAGE_KEYS = {
  CLASSES: 'mimo_business_classes',
  KEYS: 'mimo_business_teacher_keys',
  CAMPAIGNS: 'mimo_business_campaigns',
  ORGANIZATION: 'mimo_business_organization',
};

// Helper: lấy dữ liệu từ localStorage hoặc khởi tạo từ mock
function getStoredData(key, fallback) {
  try {
    const raw = localStorage.getItem(key);
    if (raw) return JSON.parse(raw);
  } catch (e) {
    console.error(`Error reading ${key}:`, e);
  }
  localStorage.setItem(key, JSON.stringify(fallback));
  return fallback;
}

function setStoredData(key, data) {
  localStorage.setItem(key, JSON.stringify(data));
}

export const businessService = {
  // 1. Lấy thông tin tổng quan trường / doanh nghiệp
  async getOverview() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.BUSINESS.OVERVIEW);
      return res;
    } catch (err) {
      console.warn('API Business Overview fallback to mock:', err);
      const org = getStoredData(STORAGE_KEYS.ORGANIZATION, mockBusinessData.organization);
      const classes = getStoredData(STORAGE_KEYS.CLASSES, mockBusinessData.classes);
      
      const totalStudents = classes.reduce((sum, c) => sum + (c.studentsCount || 0), 0);
      const completedStudents = classes.reduce((sum, c) => sum + (c.completedStudents || 0), 0);
      const complianceRate = totalStudents > 0 ? ((completedStudents / totalStudents) * 100).toFixed(1) : 0;

      return mockSuccess({
        ...org,
        metrics: {
          ...org.metrics,
          complianceRate: Number(complianceRate),
          totalStudents,
          completedStudents,
        },
      });
    }
  },

  // 2. Lấy danh sách các Khối & Lớp học
  async getClasses() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.BUSINESS.CLASSES);
      return res;
    } catch (err) {
      console.warn('API Business Classes fallback to mock:', err);
      const classes = getStoredData(STORAGE_KEYS.CLASSES, mockBusinessData.classes);
      return mockSuccess(classes);
    }
  },

  // 3. Phân công Giáo viên cho Lớp học
  async assignTeacherToClass(classId, teacherId, teacherName) {
    try {
      return await apiClient.post(API_ENDPOINTS.BUSINESS.ASSIGN_TEACHER(classId), { teacherId, teacherName });
    } catch (err) {
      console.warn('API Assign Teacher fallback to mock:', err);
      const classes = getStoredData(STORAGE_KEYS.CLASSES, mockBusinessData.classes);
      const updated = classes.map((c) => {
        if (c.id === classId) {
          return { ...c, teacherId, teacherName };
        }
        return c;
      });
      setStoredData(STORAGE_KEYS.CLASSES, updated);
      return mockSuccess(updated.find((c) => c.id === classId));
    }
  },

  // 4. Mở thêm lớp học mới
  async createClass(classData) {
    try {
      return await apiClient.post(API_ENDPOINTS.BUSINESS.CREATE_CLASS, {
        name: classData.name,
        code: classData.code,
        grade: classData.grade,
        room: classData.room,
        targetCount: Number(classData.studentsCount) || 35,
        focusTopic: classData.focusTopic,
      });
    } catch (err) {
      console.warn('API Create Class fallback to mock:', err);
      const classes = getStoredData(STORAGE_KEYS.CLASSES, mockBusinessData.classes);
      const newClass = {
        id: `cls_${Date.now()}`,
        code: classData.code || `FPT-${Math.random().toString(36).substring(2, 6).toUpperCase()}`,
        name: classData.name,
        grade: classData.grade || 'Khối 10',
        room: classData.room || 'Phòng học mới',
        studentsCount: Number(classData.studentsCount) || 35,
        completedStudents: 0,
        complianceRate: 0,
        teacherId: classData.teacherId || null,
        teacherName: classData.teacherName || 'Chưa phân công',
        status: 'ACTIVE',
        focusTopic: classData.focusTopic || 'Kỹ năng PCCC & Thoát hiểm',
      };
      const updated = [newClass, ...classes];
      setStoredData(STORAGE_KEYS.CLASSES, updated);
      return mockSuccess(newClass);
    }
  },

  // 5. Lấy danh sách Mã Bản Quyền Giáo Viên (Teacher License Keys)
  async getTeacherKeys() {
    try {
      return await apiClient.get(API_ENDPOINTS.BUSINESS.TEACHER_KEYS);
    } catch (err) {
      console.warn('API Teacher Keys fallback to mock:', err);
      const keys = getStoredData(STORAGE_KEYS.KEYS, mockBusinessData.teacherLicenseKeys);
      return mockSuccess(keys);
    }
  },

  // 6. Sinh Mã Bản Quyền Giáo Viên Mới
  async generateTeacherKey({ note, assignedClassCodes }) {
    try {
      return await apiClient.post(API_ENDPOINTS.BUSINESS.GENERATE_KEY, {
        note,
        assignedClassCodes: Array.isArray(assignedClassCodes) ? assignedClassCodes.join(',') : assignedClassCodes,
      });
    } catch (err) {
      console.warn('API Generate Teacher Key fallback to mock:', err);
      const keys = getStoredData(STORAGE_KEYS.KEYS, mockBusinessData.teacherLicenseKeys);
      const randomSuffix = Math.random().toString(36).substring(2, 6).toUpperCase();
      const newKey = {
        id: `key_${Date.now()}`,
        code: `GV-FPT-${randomSuffix}`,
        note: note || 'Mã phân công giáo viên bộ môn mới',
        assignedClassCodes: assignedClassCodes || ['FPT-10A1'],
        status: 'READY',
        activatedBy: null,
      };
      const updated = [newKey, ...keys];
      setStoredData(STORAGE_KEYS.KEYS, updated);
      return mockSuccess(newKey);
    }
  },

  // 7. Thu hồi mã bản quyền giáo viên
  async revokeTeacherKey(keyId) {
    try {
      return await apiClient.post(API_ENDPOINTS.BUSINESS.REVOKE_KEY(keyId));
    } catch (err) {
      console.warn('API Revoke Teacher Key fallback to mock:', err);
      return mockSuccess({ success: true, message: 'Đã thu hồi mã bản quyền!' });
    }
  },

  // 8. KÍCH HOẠT MÃ GIÁO VIÊN (Dành cho User nâng cấp lên EDUCATOR)
  async activateTeacherKey(inputCode, user) {
    const cleanCode = (inputCode || '').trim().toUpperCase();
    try {
      const res = await apiClient.post(API_ENDPOINTS.EDUCATOR.CLAIM_LICENSE, {
        licenseKey: cleanCode,
      });
      return res;
    } catch (err) {
      console.warn('API Claim Teacher License failed, trying local fallback:', err);
      const keys = getStoredData(STORAGE_KEYS.KEYS, mockBusinessData.teacherLicenseKeys);
      const foundIndex = keys.findIndex((k) => k.code.toUpperCase() === cleanCode);
      if (foundIndex === -1) {
        return mockError('Mã bản quyền giáo viên không hợp lệ hoặc không tồn tại!');
      }

      const targetKey = keys[foundIndex];
      if (targetKey.status === 'ACTIVATED') {
        return mockError(`Mã này đã được kích hoạt bởi ${targetKey.activatedBy?.name || 'người khác'}!`);
      }

      const updatedKey = {
        ...targetKey,
        status: 'ACTIVATED',
        activatedBy: {
          userId: user?.id || 'user_current',
          name: user?.name || 'Thầy/Cô Giáo Mới',
          email: user?.email || 'teacher@mimo.com',
          activatedAt: new Date().toLocaleString('vi-VN'),
        },
      };

      keys[foundIndex] = updatedKey;
      setStoredData(STORAGE_KEYS.KEYS, keys);

      return mockSuccess({
        message: 'Kích hoạt quyền Giáo Viên thành công!',
        role: 'EDUCATOR',
        school: 'THPT FPT Cần Thơ',
        assignedClasses: targetKey.assignedClassCodes,
      });
    }
  },

  // 9. Lấy danh sách Chiến dịch đào tạo bắt buộc
  async getCampaigns() {
    try {
      return await apiClient.get(API_ENDPOINTS.BUSINESS.CAMPAIGNS);
    } catch (err) {
      console.warn('API Business Campaigns fallback to mock:', err);
      const campaigns = getStoredData(STORAGE_KEYS.CAMPAIGNS, mockBusinessData.campaigns);
      return mockSuccess(campaigns);
    }
  },

  // 10. Tạo Chiến dịch đào tạo mới
  async createCampaign(campaignData) {
    try {
      return await apiClient.post(API_ENDPOINTS.BUSINESS.CREATE_CAMPAIGN, campaignData);
    } catch (err) {
      console.warn('API Create Campaign fallback to mock:', err);
      const campaigns = getStoredData(STORAGE_KEYS.CAMPAIGNS, mockBusinessData.campaigns);
      const newCamp = {
        id: `camp_${Date.now()}`,
        title: campaignData.title,
        target: campaignData.target || 'Toàn trường',
        assignedTopic: campaignData.assignedTopic || 'PCCC & Kỹ năng thoát hiểm',
        deadline: campaignData.deadline || '2026-11-15 17:00',
        passingScore: Number(campaignData.passingScore) || 80,
        totalAssigned: 200,
        completedCount: 0,
        complianceRate: 0,
        status: 'RUNNING',
      };
      const updated = [newCamp, ...campaigns];
      setStoredData(STORAGE_KEYS.CAMPAIGNS, updated);
      return mockSuccess(newCamp);
    }
  },

  // 11. Rung chuông nhắc nhở học sinh trong chiến dịch
  async nudgeCampaign(campaignId) {
    return mockSuccess({
      success: true,
      message: '🔔 Đã rung chuông gửi thông báo khẩn đến tất cả học sinh chưa hoàn thành!',
    });
  },

  // 12. Lấy phân tích lỗ hổng PCCC / Red flags
  async getSkillGapAnalysis() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.BUSINESS.SKILL_GAP);
      if (res?.data?.domains && Array.isArray(res.data.domains)) {
        const mapped = res.data.domains.map((d, idx) => ({
          id: d.domainId || `gap_${idx + 1}`,
          topic: d.domainName || 'Kỹ năng tổng quát',
          question: `Phân tích lỗ hổng: ${d.domainName || 'Chủ đề đào tạo'} (${d.totalWrong || 0}/${d.totalQuestionsAnswered || 0} câu sai)`,
          failRate: Math.round(d.wrongRate || 0),
          commonMistake: `Mức rủi ro: ${d.severity || 'Trung bình'}. Đã ghi nhận ${d.totalQuestionsAnswered || 0} lượt làm bài, ${d.totalWrong || 0} lượt chọn sai.`,
          expertAdvice: d.severity === 'High'
            ? 'Khuyến nghị: BGH & GV cần dành 5-10 phút sinh hoạt đầu giờ để nhắc lại quy trình chuẩn.'
            : 'Khuyến nghị: Tiếp tục giám sát và đưa vào bài kiểm tra định kỳ.',
          worstClass: 'Toàn trường',
        }));
        return { ...res, data: mapped };
      }
      if (Array.isArray(res?.data)) {
        return res;
      }
      return mockSuccess(mockBusinessData.skillGapAnalysis);
    } catch (err) {
      console.warn('API Business Skill Gap fallback to mock:', err);
      return mockSuccess(mockBusinessData.skillGapAnalysis);
    }
  },

  // 13. Lấy danh sách báo cáo kiểm định
  async getAuditReports() {
    try {
      const res = await apiClient.get(API_ENDPOINTS.BUSINESS.REPORTS);
      if (Array.isArray(res?.data)) {
        return res;
      }
      if (res?.data && typeof res.data === 'object') {
        const overview = res.data;
        const orgName = overview.organizationName || overview.OrganizationName || 'Trường THPT FPT Cần Thơ';
        const studentsCount = overview.studentsCount ?? overview.StudentsCount ?? 356;
        const complianceRate = overview.complianceRate ?? overview.ComplianceRate ?? 86.4;

        const mappedReports = [
          {
            id: overview.organizationCode || 'rep_official_2026',
            title: `Báo Cáo Kiểm Định Tuân Thủ PCCC Học Đường — ${orgName}`,
            issuedDate: new Date().toISOString().split('T')[0],
            certifiedCount: studentsCount,
            passRate: `${complianceRate}%`,
            verifier: 'Ban Kiểm định Kỹ năng Sinh tồn MIMO & Công an PCCC Cần Thơ',
            status: 'OFFICIAL_SIGNED',
          },
        ];
        return { ...res, data: mappedReports };
      }
      return mockSuccess(mockBusinessData.auditReports);
    } catch (err) {
      console.warn('API Business Reports fallback to mock:', err);
      return mockSuccess(mockBusinessData.auditReports);
    }
  },

  // 14. Thông tin thanh toán & số ghế bản quyền (Billing)
  async getBilling() {
    try {
      return await apiClient.get(API_ENDPOINTS.BUSINESS.BILLING);
    } catch (err) {
      console.warn('API Business Billing fallback to mock:', err);
      return mockSuccess({
        tier: 'Enterprise School',
        totalSeats: 500,
        usedSeats: 120,
        availableSeats: 380,
        renewalDate: '2027-10-24',
        invoices: [],
      });
    }
  },
};
