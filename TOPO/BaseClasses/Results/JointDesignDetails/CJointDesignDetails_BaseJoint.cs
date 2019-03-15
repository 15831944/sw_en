﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClasses
{
    public class CJointDesignDetails_BaseJoint : CJointDesignDetails
    {
        // Plate design

        public float fPhi_plate;
        public float fA_n_plate;
        public float fN_t_plate;
        public float fEta_N_t_5423_plate;

        public float fA_vn_yv_plate;
        public float fV_y_yv_plate;
        public float fEta_V_yv_3341_plate;

        public float fM_xu_resistance_plate;
        public float fEta_Mb_plate;

        // Shear in connection
        public float fPhi_shear_screw;
        public float fVb_MainMember;
        public int iNumberOfScrewsInShear;
        public float fEta_MainMember;

        // Plastic Design
        public float fMb_MainMember_oneside_plastic;
        public float fEta_Mb_MainMember_oneside_plastic;

        // Elastic Design
        public float fV_asterix_b_max_screw_Mxu;
        public float fV_asterix_b_max_screw_Vyv;
        public float fV_asterix_b_max_screw_N;
        public float fV_asterix_b_max_screw;

        public float fEta_Vb_5424_MainMember;

        // 5.4.2.5 Connection shear as limited by end distance
        public float fe;
        public float fV_fv_MainMember;
        public float fV_fv_Plate;
        public float fV_asterix_fv;

        public float fEta_V_fv_5425_MainMember;
        public float fEta_V_fv_5425_Plate;
        public float fEta_V_fv_5425;

        // 5.4.2.6 Screws in shear
        public float fV_w_nom_screw_5426;
        public float fEta_V_w_5426;

        // 5.4.2.3 Tension in the connected part
        public float fPhi_CrSc;
        public float fA_n_MainMember;
        public float fN_t_section_MainMember;
        public float fEta_N_t_5423_MainMember;




        // Anchors
        public float fN_asterix_joint_uplif;
        public float fN_asterix_joint_bearing;

        public float fV_asterix_x_joint;
        public float fV_asterix_y_joint;
        public float fV_asterix_res_joint;

        public int iNumberAnchors;
        public int iNumberAnchors_t;
        public int iNumberAnchors_v;

        public float fN_asterix_anchor_uplif;
        public float fV_asterix_anchor;
        public float fplateWidth_x;
        public float fplateWidth_y;

        public float fFootingDimension_x;
        public float fFootingDimension_y;
        public float fFootingHeight;

        public float fe_x_AnchorToPlateEdge;
        public float fe_y_AnchorToPlateEdge;

        public float fe_x_BasePlateToFootingEdge;
        public float fe_y_BasePlateToFootingEdge;

        public float fu_x_Washer;
        public float fu_y_Washer;

        public float ff_apostrophe_c;
        public float fRho_c;

        public float fd_s;
        public float fd_f;

        public float fA_c;
        public float fA_o;

        public float ff_y_anchor;
        public float ff_u_anchor;

        // AS / NZS 4600:2018 - 5.3 Bolted connections
        public float fPhi_v_532;
        public float fV_f_532;
        public float fEta_532_1;

        public float fPhi_v_534;
        public float fAlpha_5342;
        public float fC_5342;
        public float fV_b_5342;
        public float fEta_5342;

        public float fV_b_5343;
        public float fEta_5343;

        public float fPhi_535;
        public float fV_fv_5351_2_anchor;
        public float fEta_5351_2;

        public float fN_ft_5352_1;
        public float fEta_5352_1;

        public float fEta_5353;

        // NZS 3101.1 - 2006
        public float fElasticityFactor_1764;
        public float fPhi_anchor_tension_173;
        public float fPhi_anchor_shear_174;

        public float fPhi_concrete_tension_174a;
        public float fPhi_concrete_shear_174b;

        // 17.5.7.1  Steel strength of anchor in tension
        public float fA_se;
        public float fN_s_176_group;
        public float fEta_17571_group;

        // 17.5.7.2  Strength of concrete breakout of anchor
        public float fe_apostrophe_n;
        public float fConcreteCover;
        public float fh_ef;
        public float fs_2_x;
        public float fs_1_y;
        public float fs_min;
        public float fc_2_x;
        public float fc_1_y;
        public float fc_min;
        public float fk;
        public float fLambda_53;
        public float fPsi_1_group;
        public float fPsi_2;
        public float fPsi_3;
        public float fA_no_group;
        public float fA_n_group;
        public float fN_b_179_group;
        public float fN_b_179a_group;
        public float fN_cb_177_group;
        public float fEta_17572_group;

        // Single anchor - edge
        public float fPsi_1_single;
        public float fA_no_single;
        public float fA_n_single;
        public float fN_b_179_single;
        public float fN_b_179a_single;
        public float fN_cb_177_single;
        public float fEta_17572_single;

        // 17.5.7.3  Lower characteristic tension pullout strength of anchor
        // Group of anchors

        public float fm_x;
        public float fm_y;
        public float fA_brg;
        public float fN_p_1711_single;
        public float fPsi_4;
        public float fN_pn_1710_single;
        public float fN_pn_1710_group;
        public float fEta_17573_group;

        // 17.5.7.4 Lower characteristic concrete side face blowout strength
        // Single anchor - edge

        public float fc_1_17574;
        public float fk_1;
        public float fN_sb_1713_single;
        public float fEta_17574_single;

        // Lower characteristic strength in tension
        public float fN_n_nominal_min;

        // Lower design strength in tension
        public float fN_d_design_min;

        // 17.5.8 Lower characteristic strength of anchor in shear

        // 17.5.8.1 Lower characteristic shear strength of steel of anchor
        // Group of anchors
        public float fV_s_1714_group;
        public float fV_s_1715_group;
        public float fV_s_17581_group;
        public float fEta_17581_group;
        public float fe_apostrophe_v;
        public float fPsi_5_group;
        public float fPsi_6;
        public float fPsi_7;
        public float fA_vo;
        public float fA_v_group;
        public float fd_o;
        public float fk_2;
        public float fl;
        public float fV_b_1717a;
        public float fV_b_1717b;
        public float fV_b_1717;
        public float fV_cb_1716_group;
        public float fEta_17582_group;

        // Single of anchor - edge
        public float fPsi_5_single;
        public float fA_v_single;
        public float fV_cb_1716_single;
        public float fEta_17582_single;

        // 17.5.8.3 Lower characteristic concrete breakout strength of the anchor in shear parallel to edge
 
        // Group of anchors
        public float fV_cb_1721_group;
        public float fEta_17583_group;

        // Single anchor - edge
        public float fV_cb_1721_single;
        public float fEta_17583_single;

        // 17.5.8.4 Lower characteristic concrete pry-out of the anchor in shear
        // Group of anchors
        public float fN_cb_17584_group;
        public float fk_cp_17584;
        public float fV_cp_1722_group;
        public float fEta_17584_group;

        // Lower characteristic strength in shear
        public float fV_n_nominal_min;

        // Lower design strength in shear
        public float fV_d_design_min;

        // 17.5.6.6 Interaction of tension and shear – simplified procedures
        // Group of anchors

        // 17.5.6.6(Eq. 17–5)
        public float fEta_17566_group;

        // Footings
        public float fGamma_F_uplift;
        public float fGamma_F_bearing;
        public float fc_nominal_soil_bearing_capacity;

        // Footing pad
        public float fA_footing;
        public float fV_footing;
        public float fG_footing;

        // Tributary floor volume
        public float fG_tributary_floor;

        // Addiional material above the footing
        public float fG_additional_material;

        // Uplift
        public float fG_design_uplift;

        // Bearing
        public float fG_design_bearing;

        // Design ratio - uplift and bearing force
        public float fEta_footing_uplift;

        public float fN_design_bearing_total;
        public float fPressure_bearing;
        public float fSafetyFactor;
        public float fEta_footing_bearing;

        // Bending - design of reinforcement
        // Reinforcement bars in x direction (parallel to the wall)
        public float fq_linear_xDirection;
        public float fM_asterix_footingdesign_xDirection;

        public float fd_reinforcement_xDirection;
        public float fA_s1_Xdirection;
        public int iNumberOfBarsInXDirection;
        public float fA_s_tot_Xdirection;
        public float fSpacing_yDirection;

        public float fAlpha_c;
        public float fPhi_b_foundations;
        public float fConcreteCover_reinforcement_xDirection;
        public float fd_effective_xDirection;
        public float fx_u_xDirection;
        public float fM_b_footing_xDirection;
        public float fEta_bending_M_footing;

        // Minimum longitudinal reinforcement ratio
        public float fp_ratio_xDirection;
        public float fp_ratio_limit_minimum_xDirection;
        public float fEta_MinimumReinforcement_xDirection;

        //  Shear
        public float fV_asterix_footingdesign_shear;
        public float fA_cv_xDirection;
        public float fp_w_xDirection;
        public float fk_a;
        public float fk_d;
        public float fv_b_xDirection;
        public float fv_c_xDirection;
        public float fV_c_xDirection;
        public float fPhi_v_foundations;
        public float fEta_shear_V_footing;

        // Punching shear
        public float fcriticalPerimeter_b0;

        // Ratio of the long side to the short side of the concentrated load
        public float fBeta_c;
        public float fAlpha_s;
        public float fd_average;
        public float fk_ds;

        // Nominal shear stress resisted by the concrete
        public float fv_c_126;
        public float fv_c_127;
        public float fv_c_128;
        public float fv_c_12732;
        public float fV_c_12731;

        // 12.7.4 Shear reinforcement consisting of bars or wires or stirrups
        public float fV_s_xDirection;
        public float fV_s_yDirection;

        // 12.7.3.1 Nominal shear strength for punching shear
        public float fV_n_12731_xDirection;
        public float fEta_punching_12731_xDirection;

        public float fV_n_12731_yDirection;
        public float fEta_punching_12731_yDirection;

        public CJointDesignDetails_BaseJoint()
        {

        }
    }
}
